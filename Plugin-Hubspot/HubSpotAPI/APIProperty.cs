namespace Plugin_Hubspot.HubSpotApi
{
    public class APIProperty
    {
        public string Name { get; set; }
        
        public string Label { get; set; }
        
        public string Description { get; set; }
        
        public string GroupName { get; set; }
        
        public string Type { get; set; }
        
        public string FieldType { get; set; }
        
        public bool Deleted { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool ReadOnlyValue { get; set; }
        
        public bool ReadOnlyDefinition { get; set; }
        
        public bool Hidden { get; set; }
        
        public bool MutableDefinitionNotDeletable { get; set; }
        
        public bool Favorited { get; set; }
        
        public int FavoritedOrder { get; set; }
        
        public bool Calculated { get; set; }
        
        public bool ExternalOptions { get; set; }
        
        public string DisplayMode { get; set; }
        
        public bool FormField { get; set; }
    }
}