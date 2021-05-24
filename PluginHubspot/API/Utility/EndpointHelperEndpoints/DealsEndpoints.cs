using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class DealsEndpointHelper
    {
        private class DealsEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> DealsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllDeals", new DealsEndpoint
                {
                    Id = "AllDeals",
                    Name = "All Deals",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/deals",
                    PropertiesPath = "/crm/v3/properties/deals",
                    DetailPath = "/objects/deals/{0}",
                    DetailPropertyId = "hs_unique_creation_key",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "hs_unique_creation_key"
                    }
                }
            },
            {
                "UpsertDeals", new DealsEndpoint
                {
                    Id = "UpsertDeals",
                    Name = "Upsert Deals",
                    BasePath = "/crm/v3/objects/deals",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "hs_unique_creation_key",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Post,
                        EndpointActions.Put
                    },
                    PropertyKeys = new List<string>
                    {
                        "hs_unique_creation_key"
                    }
                }
            },
        };
    }
}