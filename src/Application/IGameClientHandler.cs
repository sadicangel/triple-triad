namespace TripleTriad;

public interface IGameClientHandler
{
    Task OnUserConnected(OnlineUser user);
    Task OnUserDisconnected(OnlineUser user);
}
