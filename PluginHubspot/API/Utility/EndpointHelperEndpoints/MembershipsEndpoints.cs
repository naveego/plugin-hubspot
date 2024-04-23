using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using PluginHubspot.API.Factory;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public static class MembershipsEndpointHelper
    {
        public class MembershipsEndpoint: Endpoint 
        {
            public override Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record, IServerStreamWriter<RecordAck> responseStream)
            {
                return base.WriteRecordAsync(apiClient, schema, record, responseStream);
            }

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Properties.Add(new Property
                {
                    Id = "recordId",
                    Name = "recordId",
                    Type = PropertyType.String,
                    IsKey = true
                });

                return Task.FromResult(schema);
            }
        }
        
        public static readonly Dictionary<string, Endpoint> MembershipsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "UpsertMemberships", new MembershipsEndpoint
                {
                    Id = "UpsertMemberships",
                    Name = "Upsert Memberships",
                    BasePath = "/crm/v3/lists/{0}/memberships/add-and-remove",
                    AllPath = "/",
                    PropertiesPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "hs_unique_creation_key",
                    ShouldGetStaticSchema = true,
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