using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginHubspot.DataContracts;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public class ApiAuthenticator: IApiAuthenticator
    {
        private HttpClient Client { get; set; }
        private IServerStreamWriter<ConnectResponse> ResponseStream { get; set; }
        private Settings Settings { get; set; }
        private string Token { get; set; }
        private DateTime ExpiresAt { get; set; }
        
        private const string AuthUrl = "https://api.hubapi.com/oauth/v1/token";
        
        public ApiAuthenticator(HttpClient client, Settings settings,IServerStreamWriter<ConnectResponse> responseStream)
        {
            Client = client;
            Settings = settings;
            ExpiresAt = DateTime.Now;
            Token = "";
            ResponseStream = responseStream;
        }

        public async Task<string> GetToken()
        {
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return Token;
            }
            
            // check if token is expired or will expire in 5 minutes or less
            if (DateTime.Compare(DateTime.Now.AddMinutes(5), ExpiresAt) >= 0)
            {
                return await GetNewToken();
            }
          
            return Token;
        }

        private async Task<string> GetNewToken()
        {
            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", Settings.ClientId),
                    new KeyValuePair<string, string>("client_secret", Settings.ClientSecret),
                    new KeyValuePair<string, string>("refresh_token", Settings.RefreshToken),
                    new KeyValuePair<string, string>("redirect_uri", Settings.RedirectUri)
                };

                var body = new FormUrlEncodedContent(formData);
                    
                var client = Client;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
                var response = await Client.PostAsync(AuthUrl, body);
                response.EnsureSuccessStatusCode();
                    
                var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
                    
                // update expiration and saved token
                ExpiresAt = DateTime.Now.AddSeconds(content.ExpiresIn);
                Token = content.AccessToken;

                var oAuthState = new OAuthState
                {
                    AuthToken = content.AccessToken,
                    RefreshToken = content.RefreshToken,
                    Config = JsonConvert.SerializeObject(new OAuthConfig
                    {
                        RedirectUri = Settings.RedirectUri
                    })
                };
                
                // update refresh token for next time
                Settings.RefreshToken = content.RefreshToken;
                
                // write connect response
                var connectResponse = new ConnectResponse
                {
                    OauthStateJson = JsonConvert.SerializeObject(oAuthState),
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = ""
                };

                await ResponseStream.WriteAsync(connectResponse);

                return Token;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}