using TripleTriad.Users.Dtos;

namespace TripleTriad.Users.Events;

public sealed class UserDisconnectedEvent : UserEvent<UserDto>
{
    public override string Type { get; init; } = "User.Disconnected";
}
