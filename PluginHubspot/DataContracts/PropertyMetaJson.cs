using Newtonsoft.Json;

namespace PluginHubspot.DataContracts
{
    public class PropertyMetaJson
    {
        [JsonProperty("hasUniqueValue")]
        public bool IsKey { get; set; }
        
        [JsonProperty("calculated")]
        public bool Calculated { get; set; }
    }
}