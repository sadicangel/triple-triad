namespace TripleTriad;

public sealed record class OnlineUser(Guid UserId, string UserName);

public static class OnlineUserMapper
{
    public static OnlineUser ToOnlineUser(this User user) => new(user.Id, user.UserName);
}
