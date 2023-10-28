using MongoDB.Driver;
using TripleTriad.Games;
using TripleTriad.Interfaces;

namespace TripleTriad.Repositories;
internal sealed class CardRepository : InMemoryRepository<Guid, Card>, ICardRepository
{
    public CardRepository(IEnumerable<Card> cards) : base(cards) { }

    public Task<List<Card>> GetRandomAsync(int count, CancellationToken cancellationToken = default)
    {
        var keys = new List<Guid>(Entities.Keys);
        return GetByIdAsync(Enumerable.Range(0, count).Select(_ => keys[Random.Shared.Next(keys.Count)]).ToList(), cancellationToken);
    }
}