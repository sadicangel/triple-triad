namespace TripleTriad.ServerState;

public sealed record class ServerStateResponse(IReadOnlyCollection<OnlineUser> Users, IReadOnlyCollection<OnlineLobby> Lobbies);
