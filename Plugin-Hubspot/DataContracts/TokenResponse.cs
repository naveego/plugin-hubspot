using Newtonsoft.Json;

namespace Plugin_Hubspot.DataContracts
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }
    }
}