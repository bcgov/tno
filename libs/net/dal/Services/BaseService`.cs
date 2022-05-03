using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;

namespace TNO.DAL.Services;

public abstract class BaseService<TEntity, TKey> : BaseService, IBaseService<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    #region Properties
    #endregion

    #region Constructors
    public BaseService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<BaseService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods

    public virtual TEntity? FindByKey(params object[] key)
    {
        return this.Context.Find<TEntity>(key);
    }

    public virtual TEntity? FindById(TKey id)
    {
        return this.Context.Find<TEntity>(id);
    }

    public virtual TEntity Add(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        this.Context.Entry(entity).State = EntityState.Added;
        this.Context.Add(entity);
        this.Context.UpdateCache<TEntity>();

        this.Context.CommitTransaction();
        return entity;
    }

    public virtual TEntity Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        this.Context.Entry(entity).State = EntityState.Modified;
        this.Context.Update(entity);
        this.Context.UpdateCache<TEntity>();

        this.Context.CommitTransaction();
        return entity;
    }

    public virtual void Delete(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        this.Context.Entry(entity).State = EntityState.Deleted;
        this.Context.Remove(entity);
        this.Context.UpdateCache<TEntity>();

        this.Context.CommitTransaction();
    }
    #endregion
}
