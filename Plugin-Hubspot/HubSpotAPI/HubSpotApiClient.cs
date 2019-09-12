using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Plugin_Hubspot.Helper;
using Pub;

namespace Plugin_Hubspot.HubSpotApi
{
    public class HubSpotApiClient
    {
        private static readonly DateTime epoch = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
        private readonly IDictionary<string, DynamicApiSchema> _schemaCache = new Dictionary<string, DynamicApiSchema>();
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

        public async Task<ApiRecords> GetRecords(DynamicObject obj, long offset = 0)
        {
            ApiRecords records = new ApiRecords();

            if (_schemaCache.TryGetValue(obj.Id, out var apiSchema) == false)
            {
                apiSchema = await GetDynamicApiSchema(obj);
                _schemaCache[obj.Id] = apiSchema;
            }

            var urlBuilder = new UriBuilder($"{ApiUrl}{obj.GetAllPath}");
            var queryString = HttpUtility.ParseQueryString(String.Empty);

            if (offset >= 0)
            {
                queryString[obj.RequestOffsetProperty] = offset.ToString();
            }

            var query = queryString.ToString();
            
            if (obj.MustRequestProperties)
            {
                foreach (var p in apiSchema.Properties)
                {
                    query += $"&properties={p.Name}";
                }
            }

            urlBuilder.Query = query;
        
            var resp = await GetAsync(urlBuilder.ToString());
            var respJson = await resp.Content.ReadAsStringAsync();

            var apiJson = JObject.Parse(respJson);

            records.HasMore = (bool)apiJson[obj.ResponseHasMoreProperty];
            records.Offset = (long)apiJson[obj.ResponseOffsetProperty];

            foreach (JObject item in apiJson[obj.ResponseDataProperty])
            {
                var data = new Dictionary<string, object>();

                // inject the id property
                data[obj.IdProp] = item[obj.IdProp].ToString();
                
                foreach (var prop in apiSchema.Properties)
                {
                    var (exists, val) = ConvertValue(item, prop);
                    if (exists)
                    {
                        data[prop.Name] = val;
                    }
                }
                
                records.Add(data);
            }
            

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

        private (bool, object) ConvertValue(JObject record, APIProperty apiProp)
        {
            var propertiesObj = (JObject) record["properties"];
            dynamic prop = propertiesObj.Properties().FirstOrDefault(p => p.Name == apiProp.Name);
            if (prop == null)
            {
                return (false, null);
            }

            var propVal = prop.Value;

            if (!(propVal is JObject))
            {
                return (false, null);
            }

            var rawVal = ((JObject) propVal)["value"];

            if (propVal.Type == JTokenType.Null)
            {
                return (true, null);
            }

            var stringVal = (string) rawVal;

            decimal n = 0;
            if(apiProp.Type == "number" && decimal.TryParse(stringVal, out n))
            {
                return (true, n);
            }

            bool b = false;
            if (apiProp.Type == "bool" && bool.TryParse(stringVal, out b))
            {
                return (true, b);
            }

            long ts = 0;
            if (apiProp.Type == "datetime" && long.TryParse(stringVal, out ts))
            {
                var dt = epoch.AddMilliseconds(ts);
                return (true, dt);
            }

            return (true, stringVal);
        }
    }
}