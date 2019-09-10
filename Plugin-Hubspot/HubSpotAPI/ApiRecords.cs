using System.Collections;
using System.Collections.Generic;

namespace Plugin_Hubspot.HubSpotApi
{
    public class ApiRecords
    {
        public List<Dictionary<string, object>> Records { get; } = new List<Dictionary<string, object>>();

        public bool HasMore { get; set; }
        
        public int Offset { get; set; }

        public void Add(Dictionary<string, object> record)
        {
            this.Records.Add(record);
        }
    }
}