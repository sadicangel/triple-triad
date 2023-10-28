using TripleTriad.Interfaces;

namespace TripleTriad.Users.Dtos;
public sealed class UserDto : IMapFrom<User>
{
    public required string Id { get; init; }

    public required string UserName { get; init; }
}
