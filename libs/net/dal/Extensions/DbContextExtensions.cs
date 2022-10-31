using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// DbContextExtensions static class, provides extension methods for DbContext objects.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// If you use the DbContext.Entry(entity).CurrentValues.SetValues(values) it will break concurrency handling.
    /// You must reset the entity Version to enable the concurrency check.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static TNOContext ResetVersion<T>(this TNOContext context, T entity)
        where T : AuditColumns
    {
        context.Entry(entity).OriginalValues[nameof(AuditColumns.Version)] = entity.Version;
        return context;
    }

    /// <summary>
    /// When updating an entity make sure the created values are not changed.
    /// If required fetch the entity from the database to get the original values.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entry"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static TNOContext OnUpdate(this TNOContext context, EntityEntry entry, ClaimsPrincipal? user)
    {
        var entity = entry.Entity as AuditColumns ?? throw new ArgumentException("Must be an entity that extends AuditColumns");
        var type = entry.Entity.GetType();

        string createdBy;
        Guid createdById;
        DateTime createdOn;
        if (entry.State == EntityState.Detached)
        {
            // Make a request to the database for the original.
            string[] keys = context.Model?.FindEntityType(type)?.FindPrimaryKey()?.Properties.Select(x => x.Name).ToArray() ?? Array.Empty<string>();
            object?[] values = keys.Select(k => type.GetProperty(k)!.GetValue(entity, null)).Where(v => v != null).ToArray();
            var original = (AuditColumns?)context.Find(type, values);
            createdBy = original?.CreatedBy ?? entity.CreatedBy;
            createdById = original?.CreatedById ?? entity.CreatedById;
            createdOn = original?.CreatedOn ?? entity.CreatedOn;
        }
        else
        {
            // These values will never be correct unless you first load the entity before updating it.
            createdBy = entry.GetOriginalValue(nameof(AuditColumns.CreatedBy), "");
            createdById = entry.GetOriginalValue(nameof(AuditColumns.CreatedById), Guid.Empty);
            createdOn = entry.GetOriginalValue(nameof(AuditColumns.CreatedOn), DateTime.UtcNow);
        }

        entity.CreatedBy = createdBy;
        entity.CreatedById = createdById;
        entity.CreatedOn = createdOn;
        entity.OnModified(user);
        return context;
    }

    /// <summary>
    /// Attempt to update cache for the cache key that matches the entity type name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static IEnumerable<Cache> UpdateCache<T>(this TNOContext context, T entity)
        where T : notnull
    {
        return context.UpdateCache(entity.GetType());
    }

    /// <summary>
    /// Will update cache if they type has the `CacheAttribute`.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static IEnumerable<Cache> UpdateCache<T>(this TNOContext context)
        where T : notnull
    {
        return context.UpdateCache(typeof(T));
    }

    /// <summary>
    /// Will update cache if they type has the `CacheAttribute`.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Cache> UpdateCache(this TNOContext context, Type type)
    {
        var result = new List<Cache>();
        var keys = type.GetCacheKeys();
        if (keys != null)
        {
            foreach (var key in keys)
            {
                var cache = context.UpdateCache(key);
                if (cache != null) result.Add(cache);
            }
        }
        return result;
    }

    /// <summary>
    /// Attempt to update cache for the specified cache key.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Cache? UpdateCache(this TNOContext context, string key)
    {
        // Update Cache
        var cache = context.Cache.Find(key);
        if (cache != null)
        {
            cache.Value = Guid.NewGuid().ToString();
            context.Update(cache);
        }
        else
        {
            context.Add(new Cache(key, Guid.NewGuid()));
        }
        return cache;
    }
}
