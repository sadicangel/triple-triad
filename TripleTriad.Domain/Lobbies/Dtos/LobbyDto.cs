using TripleTriad.Games;
using TripleTriad.Interfaces;

namespace TripleTriad.Lobbies.Dtos;
public sealed class LobbyDto : IMapFrom<Lobby>
{
    public required string Id { get; init; }

    public required string DisplayName { get; set; }

    public required string OwnerId { get; set; }

    public List<LobbyUser> Users { get; init; } = new();

    public Ruleset Rules { get; set; } = Ruleset.Default;
}
