using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Naveego.Sdk.Logging;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility;
using PluginHubspot.API.Utility.EndpointHelperEndpoints;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static async Task<string> GetSchemaJson(string [] listIds)
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
                                {"DeleteDisabled", new Dictionary<string, object>
                                {
                                    {"type", "boolean"},
                                    {"title", "Disable Delete"},
                                    {"description", "The plugin will not delete records from the memberships endpoint"},
                                    {"default", true},
                                }}
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
                                                {"title", ""},
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
                                                                {"title", "ILS ID"},
                                                                {"description", "The ILS ID of the MANUAL or SNAPSHOT list."},
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
            try
            {
                Logger.Info($"going to invoke the endpoint..");
                var response = await client.PostAsync(path, json);
                var result = await response.Content.ReadAsStringAsync();
                Logger.Info($"response for list:{result}");
                var obj = JsonConvert.DeserializeObject<dynamic>(result);
                Logger.Info($"offset:{obj?.offset}, {JsonConvert.SerializeObject(obj?.lists)}");
                var lists = obj?.lists;

                if (lists != null)
                    foreach (var list in lists)
                    {
                        listIds.Add($"{list.name.ToString()} ({list.listId.ToString()})");
                    }
            }
            catch (Exception e)
            {
                Logger.Info($"error in :{e.Message}");
                // throw;
            }

            return listIds.ToArray();
        }

    }
}