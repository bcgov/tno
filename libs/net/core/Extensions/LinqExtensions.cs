namespace TNO.Core.Extensions;
public static class LinqExtensions
{
    public static bool In<T>(this T val, params T[] values) where T : struct
    {
        return values.Contains(val);
    }
}
