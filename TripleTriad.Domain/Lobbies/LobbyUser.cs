namespace TripleTriad.Lobbies;

public sealed class LobbyUser : IEquatable<LobbyUser>
{
    public required string UserId { get; init; }

    public required string UserName { get; init; }

    public bool IsReady { get; set; }

    public bool Equals(LobbyUser? other) => other is not null && UserId == other.UserId;

    public override bool Equals(object? obj) => obj is LobbyUser user && Equals(user);

    public override int GetHashCode() => UserId.GetHashCode();

    public static bool operator ==(LobbyUser left, LobbyUser right) => left.Equals(right);

    public static bool operator !=(LobbyUser left, LobbyUser right) => !(left == right);
}
