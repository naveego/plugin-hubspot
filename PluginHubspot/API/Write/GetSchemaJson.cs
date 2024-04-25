using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PluginHubspot.API.Utility;
using PluginHubspot.API.Utility.EndpointHelperEndpoints;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static string GetSchemaJson()
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
                                                                {"description", "The ILS ID of the MANUAL or SNAPSHOT list."}
                                                            }},
                                                            {"DeleteDisabled", new Dictionary<string, object>
                                                            {
                                                                {"type", "boolean"},
                                                                {"title", "Disable Delete"},
                                                                {"default", true},
                                                                {"description", "When selected, the writeback will not delete records from the memberships endpoint"}
                                                            }}
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
    }
}