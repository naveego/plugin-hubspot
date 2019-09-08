using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin_Hubspot.DataContracts;
using Plugin_Hubspot.Helper;

namespace Plugin_Hubspot.HubSpotApi
{
    public class Authenticator
    {
        private static readonly string _authApiUrl = "https://api.hubapi.com/oauth/v1/token";
        
        private readonly HttpClient _client;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;
      
        private DateTime _expires;
        private string _token;

        public Authenticator(HttpClient client, string clientId, string clientSecret, string refreshToken)
        {
            _client = client;
            _expires = DateTime.Now;
            _token = String.Empty;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _refreshToken = refreshToken;
        }

        /// <summary>
        /// Get a token for the Salesforce API
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetToken()
        {
            // check if token is expired or will expire in 5 minutes or less
            if (DateTime.Compare(DateTime.Now.AddMinutes(5), _expires) >= 0)
            {
                try
                {

                    var formData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("client_secret", _clientSecret),
                        new KeyValuePair<string, string>("refresh_token", _refreshToken)
                    };

                    var body = new FormUrlEncodedContent(formData);
                    
                    var client = _client;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
                    var response = await _client.PostAsync(_authApiUrl, body);
                    response.EnsureSuccessStatusCode();
                    
                    var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
                    
                    // update expiration and saved token
                    _expires = DateTime.Now.AddSeconds(3600);
                    _token = content.AccessToken;

                    return _token;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    throw;
                }
            }
          
            return _token;
        }
    }
}