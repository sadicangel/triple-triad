using MongoDB.Driver;
using TripleTriad.Games;
using TripleTriad.Interfaces;

namespace TripleTriad.Repositories;
internal class GameRepository : MongoRepository<string, Game>, IGameRepository
{
    public GameRepository(IMongoDatabase database) : base(database)
    {
    }
}
