using Ardalis.Specification;
using TripleTriad.Interfaces;

namespace TripleTriad.Repositories;

internal abstract class InMemoryRepository<TId, T> : IRepository<TId, T>
    where TId : notnull
    where T : class, Interfaces.IEntity<TId>
{
    protected Dictionary<TId, T> Entities { get; }

    protected InMemoryRepository()
    {
        Entities = new Dictionary<TId, T>();
    }

    protected InMemoryRepository(IEnumerable<T> entities)
    {
        Entities = entities.ToDictionary(e => e.Id);
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Entities.Add(entity.Id, entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            Entities.Add(entity.Id, entity);
        return Task.FromResult(entities);
    }

    public Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).Any());
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Entities.Count > 0);
    }

    public Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).Count());
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Entities.Count);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Entities.Remove(entity.Id);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            Entities.Remove(entity.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        Entities.Remove(id);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(ICollection<TId> ids, CancellationToken cancellationToken = default)
    {
        foreach (var id in ids)
            Entities.Remove(id);
        return Task.CompletedTask;
    }

    public Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).FirstOrDefault());
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).FirstOrDefault());
    }

    public Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<T?>(Entities.TryGetValue(id, out var entity) ? entity : default!);
    }

    public Task<List<T>> GetByIdAsync(ICollection<TId> ids, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Entities.Keys.Where(ids.Contains).Select(k => Entities[k]).ToList());
    }

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Entities.Values.ToList());
    }

    public Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).ToList());
    }

    public Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).ToList());
    }

    public Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).SingleOrDefault());
    }

    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(specification.Evaluate(Entities.Values).SingleOrDefault());
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (!Entities.ContainsKey(entity.Id))
            throw new KeyNotFoundException();
        Entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            if (!Entities.ContainsKey(entity.Id))
                throw new KeyNotFoundException();
            Entities[entity.Id] = entity;
        }
        return Task.CompletedTask;
    }

    public Task UpsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        Entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpsertRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            Entities[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
