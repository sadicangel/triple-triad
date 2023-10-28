using TripleTriad.Lobbies.Dtos;

namespace TripleTriad.Lobbies.Events;

public sealed class UserLeftLobbyEvent : LobbyEvent<UserInLobbyDto>
{
    public override string Type { get; init; } = "Lobby.UserLeft";
}