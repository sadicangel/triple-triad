namespace TripleTriad;

public interface IGameHubHandler
{
    Task<IReadOnlyCollection<OnlineUser>> GetUsersAsync();

    Task<IReadOnlyCollection<Lobby>> GetLobbiesAsync();
    Task CreateLobbyAsync(string name);
    Task DeleteLobbyAsync(string lobbyId);
}