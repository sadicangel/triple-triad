using TripleTriad.Models;

namespace TripleTriad.Services;

public interface ITripleTriadServer : ITripleTriadUser
{
    public static ITripleTriadServer Create(Player player, int port)
    {
        return new TripleTriadServer(player, port);
    }

    public Task HostAsync(CancellationToken token);
}
