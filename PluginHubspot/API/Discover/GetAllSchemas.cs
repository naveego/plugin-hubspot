using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginHubspot.API.Factory;
using PluginHubspot.API.Utility;
using PluginHubspot.DataContracts;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClient apiClient, Settings settings,
            int sampleSize = 5)
        {
            var allEndpoints = EndpointHelper.GetAllEndpoints();

            foreach (var endpoint in allEndpoints.Values)
            {
                // base schema to be added to
                var schema = new Schema
                {
                    Id = endpoint.Id,
                    Name = endpoint.Name,
                    Description = "",
                    PublisherMetaJson = JsonConvert.SerializeObject(endpoint),
                    DataFlowDirection = endpoint.GetDataFlowDirection()
                };

                schema = await GetSchemaForEndpoint(apiClient, schema, endpoint);

                // get sample and count
                yield return await AddSampleAndCount(apiClient, schema, sampleSize, endpoint);
            }
        }

        private static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema,
            int sampleSize, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }

            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(apiClient, endpoint);

            return schema;
        }

        private static async Task<Schema> GetSchemaForEndpoint(IApiClient apiClient, Schema schema, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }

            if (endpoint.ShouldGetStaticSchema)
            {
                return await endpoint.GetStaticSchemaAsync(apiClient, schema);
            }

            // invoke properties api
            var response = await apiClient.GetAsync(endpoint.PropertiesPath);

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
                throw new Exception(error.Message);
            }

            var objectPropertiesResponse =
                JsonConvert.DeserializeObject<PropertyResponseWrapper>(
                    await response.Content.ReadAsStringAsync());

            var properties = new List<Property>();

            foreach (var objectProperty in objectPropertiesResponse.Results)
            {
                var propertyMetaJson = new PropertyMetaJson
                {
                    Calculated = objectProperty.Calculated,
                    IsKey = objectProperty.IsKey,
                    ModificationMetaData = objectProperty.ModificationMetaData
                };

                properties.Add(new Property
                {
                    Id = objectProperty.Id,
                    Name = objectProperty.Name,
                    Description = objectProperty.Description,
                    Type = GetPropertyType(objectProperty.Type),
                    TypeAtSource = objectProperty.Type,
                    IsKey = objectProperty.IsKey,
                    IsNullable = !objectProperty.IsKey,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    PublisherMetaJson = JsonConvert.SerializeObject(propertyMetaJson),
                });
            }

            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            if (schema.Properties.Count == 0)
            {
                schema.Description = Constants.EmptySchemaDescription;
            }

            schema.DataFlowDirection = endpoint.GetDataFlowDirection();

            return schema;
        }
    }
}