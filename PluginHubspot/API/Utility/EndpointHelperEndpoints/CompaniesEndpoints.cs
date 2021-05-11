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
        private class CompaniesResponse
        {
            [JsonProperty("companies")] public List<Company> Companies { get; set; }

            [JsonProperty("has-more")] public bool HasMore { get; set; }

            [JsonProperty("companyId")] public long Offset { get; set; }
        }

        private class Company
        {
            [JsonProperty("companyId")] public long CompanyId { get; set; }

            [JsonProperty("properties")] public Dictionary<string, CompanyProperty> Properties { get; set; }
        }

        private class CompanyProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }

        private class CompanyPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("description")] public string Description { get; set; }

            [JsonProperty("type")] public string Type { get; set; }
        }

        private class CompaniesEndpoint : Endpoint
        {
            private const string CompanyPropertiesPath = "/properties/v1/companies/properties/";

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                // invoke companies properties api
                var response = await apiClient.GetAsync(CompanyPropertiesPath);

                var companyProperties =
                    JsonConvert.DeserializeObject<List<CompanyPropertyMetadata>>(
                        await response.Content.ReadAsStringAsync());

                var properties = new List<Property>();

                foreach (var companyProperty in companyProperties)
                {
                    properties.Add(new Property
                    {
                        Id = companyProperty.Name,
                        Name = companyProperty.Name,
                        Description = companyProperty.Description,
                        Type = Discover.Discover.GetPropertyType(companyProperty.Type),
                        TypeAtSource = companyProperty.Type,
                        IsKey = false,
                        IsNullable = true,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                    });
                }

                properties.Add(new Property
                {
                    Id = "companyId",
                    Name = "companyId",
                    Description = "",
                    Type = PropertyType.String,
                    TypeAtSource = "",
                    IsKey = true,
                    IsNullable = false,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                });

                schema.Properties.AddRange(properties);

                return schema;
            }

            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                var countPerPage = 100;
                long offset = -1;
                var hasMore = false;

                do
                {
                    var response = await apiClient.GetAsync(
                        $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?{(offset == -1 ? "" : $"vidOffset={offset}&")}count={countPerPage}");

                    response.EnsureSuccessStatusCode();

                    var companiesResponse =
                        JsonConvert.DeserializeObject<CompaniesResponse>(await response.Content.ReadAsStringAsync());

                    hasMore = companiesResponse.HasMore;
                    offset = companiesResponse.Offset;

                    if (companiesResponse.Companies.Count == 0)
                    {
                        yield break;
                    }

                    foreach (var company in companiesResponse.Companies)
                    {
                        var recordMap = new Dictionary<string, object>();

                        recordMap["companyId"] = company.CompanyId;

                        var detailsResponse =
                            await apiClient.GetAsync($"{BasePath.TrimEnd('/')}/{string.Format(DetailPath.TrimStart('/'), company.CompanyId)}");
                        
                        detailsResponse.EnsureSuccessStatusCode();

                        var detailsCompany =
                            JsonConvert.DeserializeObject<Company>(await detailsResponse.Content.ReadAsStringAsync());

                        foreach (var companyProperty in detailsCompany.Properties)
                        {
                            recordMap[companyProperty.Key] = companyProperty.Value.Value.ToString() ?? "";
                        }

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                } while (hasMore);
            }
        }

        public static readonly Dictionary<string, Endpoint> CompaniesEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Companies", new CompaniesEndpoint
                {
                    Id = "Companies",
                    Name = "Companies",
                    BasePath = "/companies/v2/companies",
                    AllPath = "/paged",
                    DetailPath = "/{0}",
                    DetailPropertyId = "companyId",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "companyId"
                    }
                }
            }
        };
    }
}