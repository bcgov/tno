using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TNOContext OnUpdate<T>(this TNOContext context, T entity, ClaimsPrincipal? user)
        where T : AuditColumns
    {
        var current = entity;
        var entry = context.Entry(current);

        string createdBy;
        Guid createdById;
        DateTime createdOn;
        if (entry.State == EntityState.Detached)
        {
            // Make a request to the database for the original.
            string[] keys = context.Model?.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.Select(x => x.Name).ToArray() ?? Array.Empty<string>();
            object?[] values = keys.Select(k => typeof(T).GetProperty(k)!.GetValue(entity, null)).Where(v => v != null).ToArray();
            var original = (T?)context.Find(typeof(T), values);
            createdBy = original?.CreatedBy ?? entry.GetOriginalValue(nameof(entity.CreatedBy), "");
            createdById = original?.CreatedById ?? entry.GetOriginalValue(nameof(entity.CreatedById), Guid.Empty);
            createdOn = original?.CreatedOn ?? entry.GetOriginalValue(nameof(entity.CreatedOn), DateTime.UtcNow);
        }
        else
        {
            createdBy = entry.GetOriginalValue(nameof(entity.CreatedBy), "");
            createdById = entry.GetOriginalValue(nameof(entity.CreatedById), Guid.Empty);
            createdOn = entry.GetOriginalValue(nameof(entity.CreatedOn), DateTime.UtcNow);
        }

        entity.CreatedBy = createdBy;
        entity.CreatedById = createdById;
        entity.CreatedOn = createdOn;
        entity.OnModified(user);
        return context;
    }
}
