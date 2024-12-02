using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PluginHubspot.API.Utility;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static string GetSchemaJson(List<string> listIds)
        {
            // add empty string to the dropdown list
            var listIdsInDropdown = (new string[]{""}).Concat(listIds).ToArray();

            var schemaJsonObj = new Dictionary<string, object>
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
                                {"MembershipsSettings", new Dictionary<string, object>
                                {
                                    {"type", "object"},
                                    {"title", "Target membership list in Hubspot"},
                                    {"description", "Either select a target ILS list ID from the dropdown below or manually enter an ILS list ID within the field below."},
                                    {"properties", new Dictionary<string, object>
                                    {
                                        {"IlsId", new Dictionary<string, object>
                                        {
                                            {"type", "string"},
                                            {"title", "NAME OF THE HUBSPOT TARGET LIST WITH ILS ID IN PARENTHESES (LIMITED UP TO 20 RESULTS)."},
                                            {"enum", listIdsInDropdown}
                                        }},
                                        {"ManualIlsId", new Dictionary<string, object>
                                        {
                                            {"type", "string"},
                                            {"title", "MANUALLY ENTER THE ILS LIST ID."},
                                        }},
                                    }},
                                }}
                            }}
                        }}}
                    }}
                }}
            };

            return JsonConvert.SerializeObject(schemaJsonObj);
        }
    }
}