namespace TNO.Core.Extensions;

/// <summary>
/// IEnumerableExtensions static class, provides extension methods for enumerable objects.
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Iterates through the enumerable items.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            action(item);
        }
        return items;
    }
}
