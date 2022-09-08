using LiteDB;
using TripleTriad.Models;

namespace TripleTriad.Repositories;

public sealed class CardRepository : LiteDatabaseRepository<Card>, ICardRepository
{
    public CardRepository(LiteDatabase liteDatabase) : base(liteDatabase)
    {
    }
}