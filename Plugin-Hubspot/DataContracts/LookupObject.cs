using Newtonsoft.Json;

namespace Plugin_Hubspot.DataContracts
{
    public class LookupObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}