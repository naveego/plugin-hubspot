using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pub;

namespace Plugin_Hubspot.HubSpotApi
{
    public enum DynamicObject
    {
        Contacts,
        Companies,
        Deals
    }
    
    public class DynamicApiSchema : APISchema
    {

        private readonly List<APIProperty> _properties;
        
        public DynamicObject Object  { get; private set; }
        
        public string Name { get; private set; }
        
        public string Description { get; private set; }


        /// <summary>
        /// Creates an instance of a Dynamic API Schema.  For the HubSpot API
        /// the dynamic schemas are schemas that support custom properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="properties">Optional list of properties</param>
        public DynamicApiSchema(DynamicObject obj, string name, string description, List<APIProperty> properties = null)
        {
            this.Object = obj;
            this.Name = name;
            this.Description = description;
            this._properties = properties ?? new List<APIProperty>();
        }

        public void AddProperty(APIProperty property)
        {
            _properties.Add(property);
        }
        
        public override Schema ToSchema()
        {
            var s = new Schema
            {
                Id = Enum.GetName(typeof(DynamicObject), this.Object).ToLower(),
                Name = this.Name,
                Description = this.Description,
                DataFlowDirection = Schema.Types.DataFlowDirection.Read
            };

            foreach (var apiProp in this._properties)
            {
                var prop = new Property
                {
                    Id = apiProp.Name,
                    Name = apiProp.Name,
                    Description = apiProp.Description, 
                    IsNullable = true,
                    Type = GetPropertyType(apiProp.Type)
                };

                s.Properties.Add(prop);
            }

            return s;
        }
        
        private PropertyType GetPropertyType(string type)
        {
            switch (type)
            {
                case "bool":
                    return PropertyType.Bool;
                case "double":
                    return PropertyType.Float;
                case "integer":
                    return PropertyType.Integer;
                case "jsonarray":
                case "jsonobject":
                    return PropertyType.Json;
                case "date":
                case "datetime":
                    return PropertyType.Datetime;
                case "time":
                    return PropertyType.Text;
                case "float":
                    return PropertyType.Float;
                case "decimal":
                case "number":
                    return PropertyType.Decimal;
                default:
                    return PropertyType.String;
            }
        }
    }
}