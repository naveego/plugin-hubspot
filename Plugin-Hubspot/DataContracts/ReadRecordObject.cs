using Newtonsoft.Json;

namespace Plugin_Hubspot.DataContracts
{
    public class ReadRecordObject
    {
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}