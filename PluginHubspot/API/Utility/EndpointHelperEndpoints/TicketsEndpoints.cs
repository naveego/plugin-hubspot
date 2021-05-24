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
    public class TicketsEndpointHelper
    {
        private class TicketsEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> TicketsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllTickets", new TicketsEndpoint
                {
                    Id = "AllTickets",
                    Name = "All Tickets",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/tickets",
                    PropertiesPath = "/crm/v3/properties/tickets",
                    DetailPath = "/objects/tickets/{0}",
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
                "UpsertTickets", new TicketsEndpoint
                {
                    Id = "UpsertTickets",
                    Name = "Upsert Tickets",
                    BasePath = "/crm/v3/objects/tickets",
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