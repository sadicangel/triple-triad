namespace TripleTriad.Interfaces;

public interface IEntity<TId>
{
    TId Id { get; }
}
