using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;

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
        return entity;
    }

    public virtual TEntity AddAndSave(TEntity entity)
    {
        var result = Add(entity);
        this.Context.CommitTransaction();
        return result;
    }

    public virtual TEntity Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entry = this.Context.Entry(entity);
        if (entry.State == EntityState.Detached && entity is AuditColumns audit)
        {
            // Fetch the original from the database to ensure the created audit trail is not changed.
            string[] keys = this.Context.Model?.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(x => x.Name).ToArray() ?? Array.Empty<string>();
            object?[] values = keys.Select(k => typeof(TEntity).GetProperty(k)!.GetValue(entity, null)).Where(v => v != null).ToArray();
            var original = (AuditColumns?)this.Context.Find(typeof(TEntity), values);
            if (original != null)
            {
                this.Context.Entry(original).CurrentValues.SetValues(entity);
                Context.ResetVersion(original);
                this.Context.Update(original);
                this.Context.UpdateCache<TEntity>();
                return (original as TEntity)!;
            }
        }

        entry.State = EntityState.Modified;
        this.Context.Update(entity);
        this.Context.UpdateCache<TEntity>();
        return entity;
    }

    public virtual TEntity UpdateAndSave(TEntity entity)
    {
        var result = Update(entity);
        this.Context.CommitTransaction();
        return result;
    }

    public virtual void Delete(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        this.Context.Entry(entity).State = EntityState.Deleted;
        this.Context.Remove(entity);
        this.Context.UpdateCache<TEntity>();
    }

    public virtual void DeleteAndSave(TEntity entity)
    {
        Delete(entity);
        this.Context.CommitTransaction();
    }
    #endregion
}
