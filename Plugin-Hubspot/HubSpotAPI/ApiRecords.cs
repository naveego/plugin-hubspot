using System.Collections.Generic;

namespace Plugin_Hubspot.HubSpotApi
{
    public class ApiRecords
    {
        public IDictionary<string, object> Records { get; set; }
        
        public bool HasMore { get; set; }
        
        public string NextUrl { get; set; }
    }
}