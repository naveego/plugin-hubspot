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
        private readonly HubSpotApiClient _hubSpotClient;
        
        private FormSettings _formSettings;
  
        private TaskCompletionSource<bool> _tcs;
        
        public Plugin(HttpClient httpClient = null)
        {
            _hubSpotClient =  new HubSpotApiClient(httpClient ?? new HttpClient());
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
                "https://app.hubspot.com/oauth/authorize?client_id={0}&response_type={1}&redirect_uri={2}",
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
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(tokenUrl, body);
                response.EnsureSuccessStatusCode();

                var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

                oAuthState.AuthToken = content.AccessToken;
                oAuthState.RefreshToken = content.RefreshToken;
                oAuthState.Config = JsonConvert.SerializeObject(new OAuthConfig
                {
                    InstanceUrl = content.InstanceUrl
                });

                if (String.IsNullOrEmpty(oAuthState.RefreshToken))
                {
                    throw new Exception("Response did not contain a refresh token");
                }

                if (String.IsNullOrEmpty(content.InstanceUrl))
                {
                    throw new Exception("Response did not contain an instance url");
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
                _formSettings = JsonConvert.DeserializeObject<FormSettings>(request.SettingsJson);
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
            var authSuccess = await AuthorizeHttpClient();

            if (!authSuccess)
            {
                return new ConnectResponse
                {
                    ConnectionError = "Could not authenticate to API",
                    OauthError = "",
                    SettingsError = ""
                };
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

            // Resolve the dynamic Api Schemas and add them to the list
            var contactSchema = await
                _hubSpotClient.GetDynamicApiSchema(DynamicObject.Contacts, "Contacts", "HubSpot Contacts");

            var companiesSchema = await
                _hubSpotClient.GetDynamicApiSchema(DynamicObject.Companies, "Companies", "HubSpot Companies");

            var dealsSchema = await
                _hubSpotClient.GetDynamicApiSchema(DynamicObject.Deals, "Deals", "HubSpot Deals");

            discoverSchemasResponse.Schemas.Add(contactSchema.ToSchema());
            discoverSchemasResponse.Schemas.Add(companiesSchema.ToSchema());
            discoverSchemasResponse.Schemas.Add(dealsSchema.ToSchema());

            return discoverSchemasResponse;
        }

        private async Task<bool> AuthorizeHttpClient()
        {
            if (_formSettings.APIToken != null)
            {
                return true;
            }
            
            try
            {
                _hubSpotClient.UseApiToken(_formSettings.APIToken);
                await Task.Delay(0);
            }
            catch (Exception e)
            {
               Logger.Error($"Could not authenticate plugin: ${e.Message}");
            }

            return false;
        }
    }
}