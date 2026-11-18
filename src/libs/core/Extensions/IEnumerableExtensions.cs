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

    /// <summary>
    /// Iterates through the enumerable items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> items, Func<T, Task> action)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            await action(item);
        }
        return items;
    }

    /// <summary>
    /// Removes all null values from enumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereIsNotNull<T>(this IEnumerable<T> items)
    {
        return items.Where(i => i != null);
    }

    /// <summary>
    /// Append the 'add' items to a copy of the specified 'list'.
    /// This method does not change the original list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static IList<T> AppendRange<T>(this IEnumerable<T> list, IEnumerable<T> add)
    {
        var result = new List<T>(list);
        result.AddRange(add);
        return result;
    }

    /// <summary>
    /// Handle async select.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="enumeration"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<TResult>> SelectAsync<T, TResult>(
        this IEnumerable<T> enumeration, Func<T, Task<TResult>> method)
    {
        return await Task.WhenAll(enumeration.Select(async s => await method(s)));
    }

    /// <summary>
    /// Handle async select many.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="enumeration"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> enumeration, Func<T, Task<IEnumerable<TResult>>> func)
    {
        return (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);
    }
}
