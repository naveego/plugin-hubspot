using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginHubspot.DataContracts
{
    public class ObjectResponseWrapper
    {
        [JsonProperty("response")]
        public List<ObjectResponse> Results { get; set; }
        
        [JsonProperty("paging")]
        public NextResponse Paging { get; set; }
    }

    public class ObjectResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }
    }


    public class PropertyResponseWrapper
    {
        [JsonProperty("response")]
        public List<PropertyResponse> Results { get; set; }
        
        [JsonProperty("paging")]
        public NextResponse Paging { get; set; }
    }

    public class PropertyResponse
    {
        [JsonProperty("name")]
        public string Id { get; set; }
        
        [JsonProperty("label")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("hasUniqueValue")]
        public bool IsKey { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class PagingResponse
    {
        [JsonProperty("next")]
        public NextResponse Next { get; set; }
    }

    public class NextResponse
    {
        [JsonProperty("link")]
        public string Link { get; set; }
        
        [JsonProperty("after")]
        public string After { get; set; }
    }
}