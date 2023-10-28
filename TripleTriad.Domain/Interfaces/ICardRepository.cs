using TripleTriad.Games;

namespace TripleTriad.Interfaces;

public interface ICardRepository : IRepository<Guid, Card>
{
    Task<List<Card>> GetRandomAsync(int count, CancellationToken cancellationToken = default);
}