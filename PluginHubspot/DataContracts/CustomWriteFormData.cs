namespace PluginHubspot.DataContracts
{
    public class CustomWriteFormData
    {
        public Hubspot Hubspot { get; set; }
        // public string Endpoint { get; set; }
        // public EndpointSettings EndpointSettings { get; set; }
    }

    public class Hubspot
    {
        public string Endpoint { get; set; }
        public EndpointSettings EndpointSettings { get; set; }
    }

    public class EndpointSettings
    {
        public MembershipsSettings? MembershipsSettings { get; set; }
    }

    public class MembershipsSettings
    {
        public string IlsId { get; set; }
        public bool DeleteDisabled { get; set; }
    }
}