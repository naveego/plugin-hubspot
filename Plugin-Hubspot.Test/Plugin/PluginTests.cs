using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using RichardSzalay.MockHttp;
using System.Threading.Tasks;
using Grpc.Core;
using Pub;

namespace Plugin_Hubspot.Plugin
{
    public class PluginTests
    {
        
        [Fact]
        public async Task BeginOAuthFlowTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = new BeginOAuthFlowRequest()
            {
                Configuration = new OAuthConfiguration
                {
                    ClientId = "client",
                    ClientSecret = "secret",
                    ConfigurationJson = "{}"
                },
                RedirectUrl = "http://test.com"
            };

            var clientId = request.Configuration.ClientId;
            var responseType = "code";
            var redirectUrl = request.RedirectUrl;
            var prompt = "consent";
            var display = "popup";

            var authUrl = String.Format(
                "https://app.hubspot.com/oauth/authorize?client_id={0}&response_type={1}&redirect_uri={2}",
                clientId,
                responseType,
                redirectUrl);

            // act
            var response = client.BeginOAuthFlow(request);

            // assert
            Assert.IsType<BeginOAuthFlowResponse>(response);
            Assert.Equal(authUrl, response.AuthorizationUrl);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task CompleteOAuthFlowTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var completeRequest = new CompleteOAuthFlowRequest
            {
                Configuration = new OAuthConfiguration
                {
                    ClientId = "client",
                    ClientSecret = "secret",
                    ConfigurationJson = "{}"
                },
                RedirectUrl = "http://test.com?code=authcode",
                RedirectBody = ""
            };

            // act
            var response = client.CompleteOAuthFlow(completeRequest);

            // assert
            Assert.IsType<CompleteOAuthFlowResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        [Fact]
        public async Task ConnectTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        
        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();
            var disconnectRequest = new DisconnectRequest();

            // act
            var response = client.ConnectSession(request);
            var responseStream = response.ResponseStream;
            var records = new List<ConnectResponse>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
                client.Disconnect(disconnectRequest);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        [Fact]
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
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
            Assert.Equal(3, response.Schemas.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            var mockHttp = GetMockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin(mockHttp.ToHttpClient()))},
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
                ToRefresh = {new Schema {Id = "contacts"}}
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Single(response.Schemas);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        private ConnectRequest GetConnectSettings()
        {
            return new ConnectRequest
            {
                SettingsJson = "",
                OauthConfiguration = new OAuthConfiguration
                {
                    ClientId = "client",
                    ClientSecret = "secret",
                    ConfigurationJson = "{}"
                },
                OauthStateJson =
                    "{\"RefreshToken\":\"refresh\",\"AuthToken\":\"\",\"Config\":\"{\\\"InstanceUrl\\\":\\\"https://auth.hubspot.com\\\"}\"}"
            };
        }
        
        private MockHttpMessageHandler GetMockHttpMessageHandler()
        {
            //var mockHttpHelper = new MockHttpHelper();
            var mockHttp = new MockHttpMessageHandler();


            mockHttp.When("https://api.hubapi.com/oauth/v1/token")
                .Respond("application/json",
                    @"{ ""access_token"": ""xxxx"", ""refresh_token"": ""yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"", ""expires_in"": 21600 }");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/contacts/properties")
                .RespondWithJsonFile("TestData/contact.properties.json");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/companies/properties")
                .RespondWithJsonFile("TestData/companies.properties.json");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/deals/properties")
                .RespondWithJsonFile("TestData/deals.properties.json");

            return mockHttp;
        }
    }
}