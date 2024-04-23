using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static string GetUIJson()
        {
            var uiJsonObj = new Dictionary<string, object>
            {
                {"ui:order", new []
                {
                    "Hubspot"
                }},
                // {"Hubspot", new Dictionary<string, object>
                // {
                //     {"ui:order", new []{"Endpoint", "EndpointSettings"}},
                //     {"EndpointSettings", new Dictionary<string, object>
                //     {
                //         {"MembershipsSettings", new Dictionary<string, object>
                //         {
                //             {"ui:order", new []{"IlsId", "DeleteDisabled"}}
                //         }}
                //     }}
                // }}
            };
            return JsonConvert.SerializeObject(uiJsonObj);
        }
    }
}