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
    public class CompaniesEndpointHelper
    {
        private class CompaniesEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> CompaniesEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllCompanies", new CompaniesEndpoint
                {
                    Id = "AllCompanies",
                    Name = "All Companies",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/companies",
                    PropertiesPath = "/crm/v3/properties/companies",
                    DetailPath = "/objects/companies/{0}",
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
                "UpsertCompanies", new CompaniesEndpoint
                {
                    Id = "UpsertCompanies",
                    Name = "Upsert Companies",
                    BasePath = "/crm/v3/objects/companies",
                    AllPath = "/",
                    PropertiesPath = "/crm/v3/properties/companies",
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