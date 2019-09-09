using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plugin_Hubspot.Helper;
using Pub;

namespace Plugin_Hubspot.HubSpotApi
{
    public class HubSpotApiClient
    {
        private const string ApiUrl = "https://api.hubapi.com";
        private readonly HttpClient _httpClient;
        private string _apiToken = null;
        private Authenticator _authenticator;

        public HubSpotApiClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<bool> TestConnection()
        {
            await Task.Delay(0);
            return true;
        }

        public async Task<DynamicApiSchema> GetDynamicApiSchema(DynamicObject obj)
        {
            List<APIProperty> properties;
            var objName = obj.Id;
            var propertyUrl = $"{ApiUrl}/properties/v1/{objName}/properties";
            
            var resp = await GetAsync(propertyUrl);
            var stream = await resp.Content.ReadAsStreamAsync();

            var serializer = GetSerializer();
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                properties = serializer.Deserialize<List<APIProperty>>(jr);
            }
    
            return new DynamicApiSchema(obj, properties);
        }

        public async Task<ApiRecords> GetRecords(DynamicObject obj, string nextUrl = null)
        {
            ApiRecords records = new ApiRecords();

            return await Task.FromResult(records);

        }

        public void UseApiToken(string apiToken)
        {
            _apiToken = apiToken;
        }

        public void UseOAuth(string clientId, string clientSecret, string refreshToken)
        {
            _authenticator = new Authenticator(_httpClient, clientId, clientSecret, refreshToken);
        }

        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            
            if (string.IsNullOrEmpty(_apiToken) == false) {
                var builder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["hapikey"] = _apiToken;
                builder.Query = query.ToString();

                return await _httpClient.GetAsync(builder.ToString());
            }

            if (_authenticator == null)
            {
                throw new Exception("Expected OAuth Configuration");
            }

            var token = await _authenticator.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            return await _httpClient.GetAsync(uri);
        }


        private JsonSerializer GetSerializer()
        {
            return new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        
        
        
        
        
    }
}