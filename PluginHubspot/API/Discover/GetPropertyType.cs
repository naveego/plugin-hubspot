using Naveego.Sdk.Plugins;

namespace PluginHubspot.API.Discover
{
    public static partial class Discover
    {
        public static PropertyType GetPropertyType(string dataType, bool forRead = true)
        {
            if (forRead)
            {
                return PropertyType.String;
            }
            
            switch (dataType?.ToLower())
            {
                case "number":
                    return PropertyType.Float;
                case "bool":
                case "boolean":
                    return PropertyType.Bool;
                case "date":
                    return PropertyType.Date;
                case "datetime":
                case "date-time":
                    return PropertyType.Datetime;
                case "string":
                case "enumeration":
                case "phone_number":
                case "json":
                default:
                    return PropertyType.String;
            }
        }
    }
}