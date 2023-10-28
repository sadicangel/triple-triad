using TripleTriad.Lobbies.Dtos;

namespace TripleTriad.Lobbies.Events;

public sealed class LobbyUpdatedEvent : LobbyEvent<LobbyDto>
{
    public override string Type { get; init; } = "Lobby.Updated";
}