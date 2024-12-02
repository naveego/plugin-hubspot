using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public static class MembershipsEndpointHelper
    {
        public class MembershipsEndpoint : Endpoint
        {
            public override async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
                IServerStreamWriter<RecordAck> responseStream)
            {
                var recordMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(record.DataJson);
                var hasRecordId = recordMap.TryGetValue("recordId", out var recordId);
                var errorMessage = "";

                if (!hasRecordId)
                {
                    errorMessage = $"Record did not contain required property recordId";
                }
                else if (!(recordId is string _))
                {
                    errorMessage = $"Required property recordId was NULL";
                }
                else
                {
                    var json = new StringContent(
                        $"[\"{recordId}\"]",
                        Encoding.UTF8,
                        "application/json"
                    );
                    
                    var publisherInfo = JsonConvert.DeserializeObject<Hubspot>(schema.PublisherMetaJson);
                    var listId = publisherInfo.EndpointSettings.MembershipsSettings?.ParseIlsId() ?? "";
                    var url = string.Format(BasePath.TrimEnd('/'), listId);
                    Logger.Info($"list id:{listId}, record id:{recordId}, action:{record.Action}");
                    
                    HttpResponseMessage response;
                    if (record.Action == Record.Types.Action.Delete)
                    {
                        url = $"{url}/remove";
                        response = await apiClient.PutAsync(url, json);
                    }
                    else
                    {
                        url = $"{url}/add";
                        response = await apiClient.PutAsync(url, json);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var errorAck = new RecordAck
                    {
                        CorrelationId = record.CorrelationId,
                        Error = errorMessage
                    };
                    await responseStream.WriteAsync(errorAck);

                    return errorMessage;
                }

                var ack = new RecordAck
                {
                    CorrelationId = record.CorrelationId,
                    Error = ""
                };
                await responseStream.WriteAsync(ack);
                return "";
            }

            public override Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                schema.Properties.Add(new Property
                {
                    Id = "recordId",
                    Name = "Record ID",
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
                    BasePath = "/crm/v3/lists/{0}/memberships",
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