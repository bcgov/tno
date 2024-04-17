namespace TNO.Core.Extensions;

/// <summary>
/// DateTimeExtensions static class, provides extension methods for DateTime.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convert to the specified 'timeZoneId' and then return it as a local time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    public static DateTime ToTimeZone(this DateTime date, string timeZoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var result = TimeZoneInfo.ConvertTime(date, timezone);
        return DateTime.SpecifyKind(result, DateTimeKind.Local);
    }
}
