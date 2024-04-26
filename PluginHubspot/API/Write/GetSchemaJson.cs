using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static string GetSchemaJson(string [] listIds)
        {
            var schemaJsonObj = new Dictionary<string, object>
            {
                {"type", "object"},
                {"properties", new Dictionary<string, object>
                    {
                        {"Hubspot", new Dictionary<string, object>
                        {
                            {"type", "object"},
                            {"properties", new Dictionary<string, object>
                            {
                                {"Endpoint", new Dictionary<string, object>
                                {
                                    {"type", "string"},
                                    {"title", "Endpoint"},
                                    {"enum", new []{Constants.EndpointMemberships}}
                                }},
                            }},
                            {"required", new [] {"Endpoint"}},
                            {"dependencies", new Dictionary<string, object>
                            {
                                {"Endpoint", new Dictionary<string, object>
                                {
                                    {"oneOf", new []{new Dictionary<string, object>
                                    {
                                        {"properties", new Dictionary<string, object>
                                        {
                                            {"Endpoint", new Dictionary<string, object>
                                            {
                                                {"enum", new []{Constants.EndpointMemberships}}
                                            }},
                                            {"EndpointSettings", new Dictionary<string, object>
                                            {
                                                {"type", "object"},
                                                {"title", "Endpoint Settings"},
                                                {"properties", new Dictionary<string, object>
                                                {
                                                    {"MembershipsSettings", new Dictionary<string, object>
                                                    {
                                                        {"type", "object"},
                                                        {"title", ""},
                                                        {"properties", new Dictionary<string, object>
                                                        {
                                                            {"IlsId", new Dictionary<string, object>
                                                            {
                                                                {"type", "string"},
                                                                {"title", "Target membership list in Hubspot"},
                                                                {"description", "Name of the Hubspot list with ILS ID in parenthesis"},
                                                                {"enum", listIds}
                                                            }},
                                                        }},
                                                        {"required", new []{"IlsId"}}
                                                    }}
                                                }}
                                            }}
                                        }}
                                    }}}
                                }}
                            }}
                        }}
                    }
                    
                },
                {
                    "required", new[]
                    {
                        "Hubspot"
                    }
                }
            };

            return JsonConvert.SerializeObject(schemaJsonObj);
        }

        public static async Task<string[]> GetAllListId(IApiClient client)
        {
            var listIds = new List<string>();
            const string path = "crm/v3/lists/search";
            var json = new StringContent(
                "{\"listIds\":[],\"offset\":0,\"processingTypes\":[\"MANUAL\", \"SNAPSHOT\"]}",
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync(path, json);
            var result = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<dynamic>(result);
            var lists = obj?.lists;

            if (lists == null) return listIds.ToArray();
            
            foreach (var list in lists)
            {
                listIds.Add($"{list.name.ToString()} ({list.listId.ToString()})");
            }
            return listIds.ToArray();
        }

    }
}