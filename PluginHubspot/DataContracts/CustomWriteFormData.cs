namespace PluginHubspot.DataContracts
{
    public class CustomWriteFormData
    {
        public string Endpoint { get; set; }
        public MembershipsSettings? MembershipsSettings { get; set; }
    }

    public class MembershipsSettings
    {
        public string? IlsId { get; set; }
        public string? ManualIlsId { get; set; }
        public string ParseIlsId()
        {
            return IlsId![(IlsId!.LastIndexOf('(') + 1)..IlsId.LastIndexOf(')')];
        }
    }
}