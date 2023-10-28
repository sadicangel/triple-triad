using TripleTriad.Lobbies.Dtos;

namespace TripleTriad.Lobbies.Events;

public sealed class UserJoinedLobbyEvent : LobbyEvent<UserInLobbyDto>
{
    public override string Type { get; init; } = "Lobby.UserJoined";
}