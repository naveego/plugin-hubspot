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
    public class LineItemsEndpointHelper
    {
        private class LineItemsEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> LineItemsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllLineItems", new LineItemsEndpoint
                {
                    Id = "AllLineItems",
                    Name = "All Line Items",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/line_items",
                    PropertiesPath = "/crm/v3/properties/line_items",
                    DetailPath = "/objects/line_items/{0}",
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
            }
        };
    }
}