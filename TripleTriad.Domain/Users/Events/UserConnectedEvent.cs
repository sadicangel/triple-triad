using TripleTriad.Users.Dtos;

namespace TripleTriad.Users.Events;
public sealed class UserConnectedEvent : UserEvent<UserDto>
{
    public override string Type { get; init; } = "User.Connected";
}
