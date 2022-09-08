namespace TripleTriad.Repositories;

public interface IRepository<T>
{
    IEnumerable<T> FindAll();
    T? FindById(string id);
    void Insert(T entity);
    void Update(T entity);
    void Upsert(T entity);
    void Delete(string id);
}
