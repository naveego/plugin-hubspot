namespace Plugin_Hubspot.HubSpotApi
{
    public class DynamicObject
    {

        public static readonly DynamicObject Contacts = new DynamicObject
        {
            Id = "contacts",
            Name = "Contacts",
            IdProp = "vid",
            Description = "Contacts from HubSpot API",
            ResponseDataProperty = "contacts",
            ResponseHasMoreProperty = "has-more",
            ResponseOffsetProperty = "vid-offset",
            RequestOffsetProperty = "vidOffset",
            MustRequestProperties = false,
            GetAllPath = "/contacts/v1/lists/all/contacts/all"
        };
        
        public static readonly DynamicObject Companies = new DynamicObject
        {
            Id = "companies",
            Name = "Companies",
            IdProp = "companyId",
            Description = "Companies from HubSpot API",
            ResponseDataProperty = "companies",
            ResponseHasMoreProperty = "has-more",
            ResponseOffsetProperty = "offset",
            RequestOffsetProperty = "offset",
            MustRequestProperties = true,
            GetAllPath = "/companies/v2/companies/paged"
        };
        
        public static readonly DynamicObject Deals = new DynamicObject
        {
            Id = "deals",
            Name = "Deals",
            IdProp = "dealId",
            Description = "Deals from HubSpot API",
            ResponseDataProperty = "deals",
            ResponseHasMoreProperty = "hasMore",
            ResponseOffsetProperty = "offset",
            RequestOffsetProperty = "offset",
            MustRequestProperties = true,
            GetAllPath = "/deals/v1/deal/paged"
        };
        
        
        public string Id { get; set; }
        
        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The name of the Id property for the schema.
        /// </summary>
        public string IdProp { get; set; }
       
        /// <summary>
        /// A description of the object.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The property of the API response that contains the records.
        /// </summary>
        public string ResponseDataProperty { get; set; }
        
        /// <summary>
        /// The name of the property of the "has more" value. Indicating if there
        /// is another page of data.
        /// </summary>
        public string ResponseHasMoreProperty { get; set; }
        
        /// <summary>
        /// The name of the property that contains what offset 
        /// </summary>
        public string ResponseOffsetProperty { get; set; }
        
        /// <summary>
        /// The name of the Request property for supplying the offset
        /// </summary>
        public string RequestOffsetProperty { get; set; }
        
        /// <summary>
        /// Some of the HubSpot API's require that you list all the properties you want returned in the response.
        /// This indicates if that is required.  If so, they will be supplied as multiple "properties" query
        /// string parameters.
        /// </summary>
        public bool MustRequestProperties { get; set; }
        
        /// <summary>
        /// The paths used to retrieve all of the data
        /// </summary>
        public string GetAllPath { get; set; }
    }
}