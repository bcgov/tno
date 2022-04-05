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
}
