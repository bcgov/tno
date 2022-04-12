namespace TNO.DAL.Services;

public interface IBaseService<TEntity, TKey> : IBaseService
    where TEntity : class
    where TKey : notnull
{
    TEntity? FindById(TKey id);
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
}
