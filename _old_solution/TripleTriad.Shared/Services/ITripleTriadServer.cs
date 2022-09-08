using CommunityToolkit.Mvvm.Messaging;
using TripleTriad.Models;

namespace TripleTriad.Services;

public interface ITripleTriadServer : ITripleTriadUser
{
    public static ITripleTriadServer Create(IMessenger messenger, Player player, int port)
    {
        return new TripleTriadServer(messenger, player, port);
    }

    public Task HostAsync(CancellationToken token);
}
