using Grpc.Core;
using Naveego.Sdk.Plugins;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public interface IApiClientFactory
    {
        void InitializeApiClientFactory(IServerStreamWriter<ConnectResponse> responseStream);
        IApiClient CreateApiClient(Settings settings);
    }
}