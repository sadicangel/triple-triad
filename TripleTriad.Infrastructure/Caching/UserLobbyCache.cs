using Microsoft.Extensions.Caching.Distributed;
using TripleTriad.Interfaces;

namespace TripleTriad.Caching;
internal sealed class UserLobbyCache : RedisCache<string>, IUserLobbyCache
{
    public override string KeyPrefix { get; } = "UserLobby_";

    public UserLobbyCache(IDistributedCache cache) : base(cache) { }
}
