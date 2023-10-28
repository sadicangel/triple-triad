namespace Domain;

public interface IGameClient
{
    Task OnUserConnected(string connectionId);
    Task OnUserDisconnected(string connectionId);
    Task OnMessageReceived(string username, string message);
}
