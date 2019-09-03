using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pub;
using Xunit;
using Record = Pub.Record;

namespace Plugin_Hubspot.Plugin
{
    public class PluginTests
    {
        private ConnectRequest GetConnectSettings()
        {
            
            return new ConnectRequest
            {
                SettingsJson = "{\"OAuthClientID\":\"123\",\"OAuthClientSecret\":\"456\",\"Username\":\"test\",\"Password\": \"test\"}"
            };
        }
        
        [Fact]
        public async Task ConnectTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = new ConnectRequest
            {
                SettingsJson = "{\"OAuthClientID\":\"\",\"OAuthClientSecret\":\"\",\"Username\":\"\",\"Password\": \"\"}"
            };

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);
            Assert.True(string.IsNullOrEmpty(response.OauthError), "OAuthError not null or empty");
            Assert.True(string.IsNullOrEmpty(response.ConnectionError), $"ConnectionError not null or empty:{response.ConnectionError}");
            Assert.True(string.IsNullOrEmpty(response.SettingsError), $"SettingsError not null or empty:{response.SettingsError}");

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.True(response.Schemas.Count == 5, "Schema Length was not 5");
            
            Assert.True(response.Schemas.Any(s => s.Id == "WELL"), "Missing Schema WELL");
            Assert.True(response.Schemas.Any(s => s.Id == "WELLBORE"), "Missing Schema WELLBORE");
            Assert.True(response.Schemas.Any(s => s.Id == "COMPLETION"), "Missing Schema COMPLETION");
            Assert.True(response.Schemas.Any(s => s.Id == "PAD"), "Missing Schema PAD");
            Assert.True(response.Schemas.Any(s => s.Id == "FACILITY"), "Missing Schema FACILITY");

            var wellSchema = response.Schemas.First(s => s.Id == "WELL");
            Assert.True(wellSchema.Properties.Count == 36, $"Expected WELL property count to be 36 but was {wellSchema.Properties.Count}");

            var wellBoreSchema = response.Schemas.First(s => s.Id == "WELLBORE");
            Assert.True(wellBoreSchema.Properties.Count == 25, $"Expected WELLBORE property count to be 25 but was {wellBoreSchema.Properties.Count}");
            
            var completionSchema = response.Schemas.First(s => s.Id == "COMPLETION");
            Assert.True(completionSchema.Properties.Count == 25, $"Expected COMPLETION property count to be 25 but was {completionSchema.Properties.Count}");
            
            var padSchema = response.Schemas.First(s => s.Id == "PAD");
            Assert.True(padSchema.Properties.Count == 15, $"Expected PAD property count to be 15 but was {padSchema.Properties.Count}");
            
            var facilitySchema = response.Schemas.First(s => s.Id == "FACILITY");
            Assert.True(facilitySchema.Properties.Count == 8, $"Expected FACILITY property count to be 8 but was {facilitySchema.Properties.Count}");

            
            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {new Schema {Id = "WELL"}}
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.True(response.Schemas.Count == 1, $"Count was not 1 but was {response.Schemas.Count}");
            Assert.True(response.Schemas[0].Id == "WELL", "ID was not well");
            

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new ReadRequest()
            {
                Schema = new Schema{ Name = "WELL" }
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(25, records.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        [Fact]
        public async Task ReadStreamTestChangeNewLinetoCarriageReturnLineFeed()
        {
            // setup        
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();
            var schema = new Schema {Name = "WELL"};
            schema.Properties.Add(new Property{ Id = "WELL_NAME", Name = "WELL_NAME", Type = PropertyType.String });

            var request = new ReadRequest()
            {
                Schema = schema
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            var firstRecord = records.First();
            var firstRecordData = JObject.Parse(firstRecord.DataJson);
            Assert.Equal("ADAMS-02HU\r\n", (string)firstRecordData["WELL_NAME"]);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        
        [Fact]
        public async Task ReadStreamTestSetDecimalPrecision()
        {
            // setup        
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Hubspot.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();
            var schema = new Schema {Name = "WELL"};
            schema.Properties.Add(new Property{ Id = "WELL_NAME", Name = "WELL_NAME", Type = PropertyType.String });
            schema.Properties.Add(new Property{ Id = "GROUND_ELEVATION_FT", Name = "GROUND_ELEVATION_FT", Type = PropertyType.Decimal });

            var request = new ReadRequest()
            {
                Schema = schema
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            var firstRecord = records.First();
            var firstRecordData = JObject.Parse(firstRecord.DataJson);
            Assert.Equal("1325.00000", firstRecordData["GROUND_ELEVATION_FT"].ToString());

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
       
    }
}