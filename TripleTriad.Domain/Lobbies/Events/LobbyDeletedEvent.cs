using TripleTriad.Lobbies.Dtos;

namespace TripleTriad.Lobbies.Events;

public sealed class LobbyDeletedEvent : LobbyEvent<LobbyDto>
{
    public override string Type { get; init; } = "Lobby.Deleted";
}