namespace TripleTriad;

public interface IGameClientHandler
{
    Task OnUserConnected(OnlineUser user);
    Task OnUserDisconnected(OnlineUser user);
    Task OnLobbyCreated(Lobby lobby);
    Task OnLobbyRemoved(Lobby lobby);
}
