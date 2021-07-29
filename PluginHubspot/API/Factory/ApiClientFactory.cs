using System.Net.Http;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public class ApiClientFactory: IApiClientFactory
    {
        private HttpClient Client { get; set; }
        private IServerStreamWriter<ConnectResponse> ResponseStream { get; set; }

        public ApiClientFactory(HttpClient client)
        {
            Client = client;
        }

        public void InitializeApiClientFactory(IServerStreamWriter<ConnectResponse> responseStream)
        {
            ResponseStream = responseStream;
        }

        public IApiClient CreateApiClient(Settings settings)
        {
            return new ApiClient(Client, settings, ResponseStream);
        }
    }
}