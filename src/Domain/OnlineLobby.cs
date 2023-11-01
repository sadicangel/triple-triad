namespace TripleTriad;

public sealed record class OnlineLobby(Guid LobbyId, string Name, Guid Owner, List<Guid> Users, string Rules);

public static class OnlineLobbyMapper
{
    public static OnlineLobby ToOnlineLobby(this Lobby lobby) => new(lobby.Id, lobby.Name, lobby.Owner, lobby.Users, lobby.Rules);
}