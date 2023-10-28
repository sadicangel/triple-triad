using TripleTriad.Users.Dtos;

namespace TripleTriad.Lobbies.Dtos;
public sealed class UserInLobbyDto
{
    public required LobbyDto Lobby { get; init; }
    public required UserDto User { get; init; }
}
