using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Google.Protobuf.Collections;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin_Hubspot.DataContracts;
using Plugin_Hubspot.Helper;
using Plugin_Hubspot.HubSpotApi;
using Pub;

namespace Plugin_Hubspot.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private readonly HttpClient _httpClient;
        private readonly HubSpotApiClient _hubSpotClient;
     
        private FormSettings _formSettings;
  
        private TaskCompletionSource<bool> _tcs;

        private volatile bool _connected;
        
        public Plugin(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _hubSpotClient =  new HubSpotApiClient(_httpClient);
        }

        public override Task<BeginOAuthFlowResponse> BeginOAuthFlow(BeginOAuthFlowRequest request, ServerCallContext context)
        {
            Logger.Info("Getting Auth URL...");
            
            
            // params for auth url
            var clientId = request.Configuration.ClientId;
            var responseType = "code";
            var redirectUrl = request.RedirectUrl;


            // build auth url
            var authUrl = String.Format(
                "https://app.hubspot.com/oauth/authorize?client_id={0}&response_type={1}&redirect_uri={2}&scope=contacts%20forms",
                clientId,
                responseType,
                redirectUrl);

            // return auth url
            var oAuthResponse = new BeginOAuthFlowResponse
            {
                AuthorizationUrl = authUrl
            };

            Logger.Info($"Created Auth URL: {authUrl}");

            return Task.FromResult(oAuthResponse);
        }

        public override async Task<CompleteOAuthFlowResponse> CompleteOAuthFlow(CompleteOAuthFlowRequest request, ServerCallContext context)
        {
            Logger.Info("Getting Auth and Refresh Token...");

            string code;
            var uri = new Uri(request.RedirectUrl);

            try
            {
                code = HttpUtility.UrlDecode(HttpUtility.ParseQueryString(uri.Query).Get("code"));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
            
            // token url parameters
            var redirectUrl = String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority,
                uri.AbsolutePath);
            var clientId = request.Configuration.ClientId;
            var clientSecret = request.Configuration.ClientSecret;
            var grantType = "authorization_code";

            // build token url
            var tokenUrl = "https://api.hubapi.com/oauth/v1/token";

            // build form data request
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", grantType),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                new KeyValuePair<string, string>("code", code)
            };
            
            var body = new FormUrlEncodedContent(formData);

            // get tokens
            var oAuthState = new OAuthState();
            try
            {
                var response = await _httpClient.PostAsync(tokenUrl, body);
                response.EnsureSuccessStatusCode();

                var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

                oAuthState.AuthToken = content.AccessToken;
                oAuthState.RefreshToken = content.RefreshToken;

                if (String.IsNullOrEmpty(oAuthState.RefreshToken))
                {
                    throw new Exception("Response did not contain a refresh token");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // return oauth state json
            var oAuthResponse = new CompleteOAuthFlowResponse
            {
                OauthStateJson = JsonConvert.SerializeObject(oAuthState)
            };

            Logger.Info("Got Auth Token and Refresh Token");

            return oAuthResponse;

        }

        /// <summary>
        /// Establishes a connection with Naveego Legacy CRM. Creates an authenticated http client and tests it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>A message indicating connection success</returns>
        public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            try
            {
                _formSettings = JsonConvert.DeserializeObject<FormSettings>(request.SettingsJson) ?? new FormSettings();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return new ConnectResponse
                {
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }

            // create new authenticated request helper with validated settings
           
            if (string.IsNullOrEmpty(_formSettings.APIToken) == false)
            {
                _hubSpotClient.UseApiToken(_formSettings.APIToken);
            }
            else
            {
                try
                {
                    var oAuthState = JsonConvert.DeserializeObject<OAuthState>(request.OauthStateJson);
                    _hubSpotClient.UseOAuth(
                        request.OauthConfiguration.ClientId,
                        request.OauthConfiguration.ClientSecret,
                        oAuthState.RefreshToken
                    );
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    return new ConnectResponse
                    {
                        OauthStateJson = request.OauthStateJson,
                        ConnectionError = "",
                        OauthError = e.Message,
                        SettingsError = ""
                    };
                }
            }

            // attempt to call the Legacy API api
            try
            {
                await _hubSpotClient.TestConnection();
                Logger.Info("Successfully Connected to HubSpot API");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);

                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = e.Message,
                    OauthError = "",
                    SettingsError = ""
                };
            }

            _connected = true;

            return new ConnectResponse
            {
                ConnectionError = "",
                OauthError = "",
                SettingsError = ""
            };
        }

        public override async Task ConnectSession(ConnectRequest request,
            IServerStreamWriter<ConnectResponse> responseStream, ServerCallContext context)
        {
            Logger.Info("Connecting session...");

            // create task to wait for disconnect to be called
            _tcs?.SetResult(true);
            _tcs = new TaskCompletionSource<bool>();

            // call connect method
            var response = await Connect(request, context);

            await responseStream.WriteAsync(response);

            Logger.Info("Session connected.");

            // wait for disconnect to be called
            await _tcs.Task;
        }

        /// <summary>
        /// Handles disconnect requests from the agent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            // alert connection session to close
            if (_tcs != null)
            {
                _tcs.SetResult(true);
                _tcs = null;
            }

            _connected = false;

            Logger.Info("Disconnected");
            return Task.FromResult(new DisconnectResponse());
        }


        /// <summary>
        /// Discovers schemas located in the users Zoho CRM instance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Discovered schemas</returns>
        public override async Task<DiscoverSchemasResponse> DiscoverSchemas(DiscoverSchemasRequest request,
            ServerCallContext context)
        {
            Logger.Info("Discovering Schemas...");

            DiscoverSchemasResponse discoverSchemasResponse = new DiscoverSchemasResponse();

            var schemasToLoad = (request.ToRefresh.Count > 0)
                ? request.ToRefresh.Select(s => s.Id).ToArray()
                : new[] {"contacts", "companies", "deals"};

            try
            {
                // Resolve the dynamic Api Schemas and add them to the list
                if (schemasToLoad.Contains("contacts"))
                {
                    var contactSchema = await
                        _hubSpotClient.GetDynamicApiSchema(DynamicObject.Contacts);

                    discoverSchemasResponse.Schemas.Add(contactSchema.ToSchema());
                }

                if (schemasToLoad.Contains("companies"))
                {
                    var companiesSchema = await
                        _hubSpotClient.GetDynamicApiSchema(DynamicObject.Companies);

                    discoverSchemasResponse.Schemas.Add(companiesSchema.ToSchema());
                }

                if (schemasToLoad.Contains("deals"))
                {
                    var dealsSchema = await
                        _hubSpotClient.GetDynamicApiSchema(DynamicObject.Deals);

                    discoverSchemasResponse.Schemas.Add(dealsSchema.ToSchema());
                }
                
                return discoverSchemasResponse;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not discover schemas: " + ex.ToString());
                throw;
            }  
        }
        
        /// <summary>
        /// Publishes a stream of data for a given schema
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ReadStream(ReadRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            var schema = request.Schema;
            var dynamicObject = DynamicObject.GetByName(schema.Id);
            var limit = request.Limit;
            var limitFlag = request.Limit != 0;

            Logger.Info($"Publishing records for schema: {schema.Name}");

            try
            {
                var recordsCount = 0;
                var records = new List<Dictionary<string, object>>();

                // get all records
                // build query string
                StringBuilder query = new StringBuilder("select+");

                foreach (var property in schema.Properties)
                {
                    query.Append($"{property.Id},");
                }

                // remove trailing comma
                query.Length--;

                query.Append($"+from+{schema.Id}");

                ApiRecords apiRecords;
                long offset = 0;

                do
                {
                    // get records for schema page by page
                    apiRecords = await _hubSpotClient.GetRecords(dynamicObject, offset);
                    records.AddRange((apiRecords.Records));
                    offset = apiRecords.Offset;
                } while (apiRecords.HasMore && _connected);

                // Publish records for the given schema
                foreach (var record in records)
                {
                    var recordOutput = new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(record)
                    };

                    // stop publishing if the limit flag is enabled and the limit has been reached
                    if ((limitFlag && recordsCount == limit) || !_connected)
                    {
                        break;
                    }

                    // publish record
                    await responseStream.WriteAsync(recordOutput);
                    recordsCount++;
                }

                Logger.Info($"Published {recordsCount} records");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

       
        
    }
}