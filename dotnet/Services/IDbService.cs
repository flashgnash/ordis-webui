public interface IDbService<T, TKey>
{
    void Insert(T item);
    void Update(T item);
    void Delete(TKey id);
    T? GetById(TKey id);
}
