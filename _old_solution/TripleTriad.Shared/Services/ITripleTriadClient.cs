using CommunityToolkit.Mvvm.Messaging;
using Grpc.Net.Client;
using TripleTriad.Models;

namespace TripleTriad.Services;

public interface ITripleTriadClient : ITripleTriadUser
{
    public static ITripleTriadClient Create(IMessenger messenger, Player player, string address)
    {
        var options = new GrpcChannelOptions
        {
            HttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }
        };
        return new TripleTriadClient(messenger, player, GrpcChannel.ForAddress(address, options));
    }

    public Task<Player> JoinAsync(CancellationToken token);
}
