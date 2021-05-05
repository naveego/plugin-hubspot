using System.Collections.Generic;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public static class BouncesEndpointHelper
    {
        private class BouncesEndpoint : Endpoint
        {
        }
        
        public static readonly Dictionary<string, Endpoint> BouncesEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllBounces", new BouncesEndpoint
            {
                Id = "AllBounces",
                Name = "All Bounces",
                BasePath = "/Bounces",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "BounceID"
                }
            }},
        };
    }
}