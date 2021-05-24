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
    public class ProductsEndpointHelper
    {
        private class ProductsEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> ProductsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllProducts", new ProductsEndpoint
                {
                    Id = "AllProducts",
                    Name = "All Products",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/products",
                    PropertiesPath = "/crm/v3/properties/products",
                    DetailPath = "/objects/products/{0}",
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
                "UpsertProducts", new ProductsEndpoint
                {
                    Id = "UpsertProducts",
                    Name = "Upsert Products",
                    BasePath = "/crm/v3/objects/products",
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