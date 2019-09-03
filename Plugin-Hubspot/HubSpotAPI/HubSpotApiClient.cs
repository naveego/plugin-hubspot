using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

        public HubSpotApiClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<bool> TestConnection()
        {
            await Task.Delay(0);
            return true;
        }

        public async Task<DynamicApiSchema> GetDynamicApiSchema(DynamicObject obj, string name, string description)
        {
            List<APIProperty> properties;
            var objName = Enum.GetName(typeof(DynamicObject), obj).ToLower();
            var propertyUrl = $"{ApiUrl}/properties/v1/{objName}/properties";
            
            var resp = await _httpClient.GetAsync(propertyUrl);
            var stream = await resp.Content.ReadAsStreamAsync();

            var serializer = GetSerializer();
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                properties = serializer.Deserialize<List<APIProperty>>(jr);
            }
    
            return new DynamicApiSchema(obj, name, description, properties);
        }

        public void UseApiToken(string apiToken)
        {
            _apiToken = apiToken;
        }

        private async Task<HttpResponseMessage> GET(string uri)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["hapikey"] = _apiToken;
            builder.Query = query.ToString();

            return await _httpClient.GetAsync(builder.ToString());
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