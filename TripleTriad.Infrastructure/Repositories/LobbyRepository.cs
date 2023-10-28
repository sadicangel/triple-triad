using MongoDB.Driver;
using TripleTriad.Interfaces;
using TripleTriad.Lobbies;

namespace TripleTriad.Repositories;

internal sealed class LobbyRepository : MongoRepository<string, Lobby>, ILobbyRepository
{
    public LobbyRepository(IMongoDatabase database) : base(database) { }
}