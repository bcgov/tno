namespace TNO.DAL.Services;

/// <summary>
/// IBaseService interface, provides common methods for services.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IBaseService<TEntity, TKey> : IBaseService
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Find one entity for the specified primary key values.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    TEntity? FindByKey(params object[] key);

    /// <summary>
    /// Find one entity for the specified primary key value.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    TEntity? FindById(TKey id);

    /// <summary>
    /// Add the specified 'entity' to the context so that it can be added to the database.
    /// This operation does not commit the transaction to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Add(TEntity entity);

    /// <summary>
    /// Add the specified 'entity' to the database as a transaction.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity AddAndSave(TEntity entity);

    /// <summary>
    /// Update the specified 'entity' in the context so that it can be updated in the database.
    /// This operation does not commit the transaction to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// Update the specified 'entity' in the database as a transaction.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity UpdateAndSave(TEntity entity);

    /// <summary>
    /// Remove the specified 'entity' from context so that it can be removed from the database.
    /// This operation does not commit the transaction to the database.
    /// </summary>
    /// <param name="entity"></param>
    void Delete(TEntity entity);

    /// <summary>
    /// Remove the specified 'entity' from the database as a transaction.
    /// </summary>
    /// <param name="entity"></param>
    void DeleteAndSave(TEntity entity);
}
