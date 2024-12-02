using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginHubspot.API.Write
{
    public static partial class Write
    {
        public static string GetUIJson()
        {
            var uiJsonObj = new Dictionary<string, object>{};
            
            return JsonConvert.SerializeObject(uiJsonObj);
        }
    }
}