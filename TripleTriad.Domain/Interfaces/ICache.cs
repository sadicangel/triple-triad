namespace TripleTriad.Interfaces;

public interface ICache<TValue>
{
    string KeyPrefix { get; }
    Task<TValue?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, TValue value, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
