namespace TripleTriad;

public sealed record class Lobby(string LobbyId, string DisplayName, string Owner, List<string> Users);
