using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;

namespace TNO.DAL.Extensions;

/// <summary>
/// IEnumerableExtensions static class, provides extension methods for enumerable objects.
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Iterates over the enumerable array and changes the entity state of each entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities"></param>
    /// <param name="context"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static IEnumerable<T> SetEntityState<T>(this IEnumerable<T> entities, TNOContext context, EntityState state = EntityState.Added)
        where T : notnull
    {
        entities.ForEach(a =>
        {
            context.Entry(a).State = state;
        });
        return entities;
    }
}
