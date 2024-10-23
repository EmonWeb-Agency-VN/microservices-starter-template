using Grpc.Net.Client;

namespace Common.Proxies.Interfaces
{
    public interface IGrpcChannelFactory
    {
        GrpcChannel CreateChannel(string endpoint);
    }
}
