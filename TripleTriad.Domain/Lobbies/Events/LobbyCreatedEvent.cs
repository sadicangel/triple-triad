using TripleTriad.Lobbies.Dtos;

namespace TripleTriad.Lobbies.Events;

public sealed class LobbyCreatedEvent : LobbyEvent<LobbyDto>
{
    public override string Type { get; init; } = "Lobby.Created";
}