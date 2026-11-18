using System.ComponentModel.DataAnnotations;

namespace TNO.Core.Extensions;

/// <summary>
/// EnumExtensions static class, provides extension methods for enum values.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get the enum name value of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? GetName<T>(this T value)
    {
        if (value == null) return null;

        var enumType = typeof(T);
        var memberInfos = enumType.GetMember(value.ToString() ?? "");
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType) ?? throw new InvalidOperationException("Invalid enum type");
        var attribute = (DisplayAttribute?)enumValueMemberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
        return attribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Convert flagged enum into an array of int.
    /// A default value with a value of '0' can be excluded when there are other values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="includeZero"></param>
    /// <returns></returns>
    public static int[] GetFlagValues<T>(this T value, bool includeZero = true)
        where T : Enum
    {
        var values = Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(v => value.HasFlag(v))
            .Select(v => (int)(object)v)
            .ToArray();

        if (values.Length > 1 && !includeZero) return values.Where((v) => v != 0).ToArray();
        return values;
    }

    /// <summary>
    /// Convert array of int into flagged enum.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToFlagEnum<T>(this IEnumerable<int> value)
        where T : Enum
    {
        return (T)(object)value.Aggregate(0, (c, n) => c |= n);
    }

    /// <summary>
    /// Return a new enumerable by extracting all items that are null or empty or whitespace.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> NotNullOrWhiteSpace<T>(this IEnumerable<T?> items)
    {
        return items.Where(v => v != null && !String.IsNullOrWhiteSpace($"{v}")).Select(v => v!);
    }
}
