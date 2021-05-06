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
    public class ContactsEndpointHelper
    {
        private class ContactsResponse
        {
            [JsonProperty("contacts")] public List<Contact> Contacts { get; set; }

            [JsonProperty("has-more")] public bool HasMore { get; set; }

            [JsonProperty("vid-offset")] public long VidOffset { get; set; }
        }

        private class Contact
        {
            [JsonProperty("vid")] public long Vid { get; set; }

            [JsonProperty("properties")] public Dictionary<string, ContactProperty> Properties { get; set; }
        }

        private class ContactProperty
        {
            [JsonProperty("value")] public object Value { get; set; }
        }

        private class ContactPropertyMetadata
        {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("description")] public string Description { get; set; }

            [JsonProperty("type")] public string Type { get; set; }
        }

        private class ContactsEndpoint : Endpoint
        {
            private const string ContactPropertiesPath = "/properties/v1/contacts/properties";

            public override bool ShouldGetStaticSchema { get; set; } = true;

            public override async Task<Schema> GetStaticSchemaAsync(IApiClient apiClient, Schema schema)
            {
                // invoke contacts properties api
                var response = await apiClient.GetAsync(ContactPropertiesPath);

                var contactProperties =
                    JsonConvert.DeserializeObject<List<ContactPropertyMetadata>>(
                        await response.Content.ReadAsStringAsync());

                var properties = new List<Property>();

                foreach (var contactProperty in contactProperties)
                {
                    properties.Add(new Property
                    {
                        Id = contactProperty.Name,
                        Name = contactProperty.Name,
                        Description = contactProperty.Description,
                        Type = Discover.Discover.GetPropertyType(contactProperty.Type),
                        TypeAtSource = contactProperty.Type,
                        IsKey = false,
                        IsNullable = true,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                    });
                }

                properties.Add(new Property
                {
                    Id = "vid",
                    Name = "vid",
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
                long vidOffset = -1;
                var hasMore = false;

                do
                {
                    var response = await apiClient.GetAsync(
                        $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}?{(vidOffset == -1 ? "" : $"vidOffset={vidOffset}&")}count={countPerPage}");

                    response.EnsureSuccessStatusCode();

                    var contactsResponse =
                        JsonConvert.DeserializeObject<ContactsResponse>(await response.Content.ReadAsStringAsync());

                    hasMore = contactsResponse.HasMore;
                    vidOffset = contactsResponse.VidOffset;

                    if (contactsResponse.Contacts.Count == 0)
                    {
                        yield break;
                    }

                    foreach (var contact in contactsResponse.Contacts)
                    {
                        var recordMap = new Dictionary<string, object>();

                        recordMap["vid"] = contact.Vid;

                        var detailsResponse =
                            await apiClient.GetAsync(string.Format(DetailPath.TrimStart('/'), contact.Vid));
                        
                        detailsResponse.EnsureSuccessStatusCode();

                        var detailsContact =
                            JsonConvert.DeserializeObject<Contact>(await detailsResponse.Content.ReadAsStringAsync());

                        foreach (var contactProperty in detailsContact.Properties)
                        {
                            recordMap[contactProperty.Key] = contactProperty.Value.Value.ToString() ?? "";
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

        public static readonly Dictionary<string, Endpoint> ContactsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "Contacts", new ContactsEndpoint
                {
                    Id = "Contacts",
                    Name = "Contacts",
                    BasePath = "/contacts/v1",
                    AllPath = "/lists/all/contacts/all",
                    DetailPath = "/contact/vid/{0}/profile",
                    DetailPropertyId = "vid",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "vid"
                    }
                }
            }
        };
    }
}