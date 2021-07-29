using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using PluginHubspot.API.Utility;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public class ApiClient: IApiClient
    {
        private IApiAuthenticator Authenticator { get; set; }
        private static HttpClient Client { get; set; }
        private IServerStreamWriter<ConnectResponse> ResponseStream { get; set; }
        private Settings Settings { get; set; }

        private const string ApiKeyParam = "hapikey";

        public ApiClient(HttpClient client, Settings settings, IServerStreamWriter<ConnectResponse> responseStream)
        {
            Authenticator = new ApiAuthenticator(client, settings, responseStream);
            Client = client;
            Settings = settings;
            ResponseStream = responseStream;
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task TestConnection()
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{Utility.Constants.TestConnectionPath.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = json
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = uri,
                    Content = json
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = uri,
                    Content = json
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.BaseApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    query[ApiKeyParam] = Settings.ApiKey;
                }
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = uri
                };

                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}