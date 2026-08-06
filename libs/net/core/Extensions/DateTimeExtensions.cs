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
        var timezone = ResolveTimeZone(timeZoneId);
        var result = TimeZoneInfo.ConvertTime(date, timezone);
        return DateTime.SpecifyKind(result, DateTimeKind.Local);
    }

    /// <summary>
    /// Resolve a Windows or IANA time zone id to a TimeZoneInfo, tolerant of invalid or legacy
    /// values. When the id cannot be resolved directly it is converted between Windows and IANA
    /// forms; UTC aliases/display names are handled explicitly. Falls back to UTC (never throws)
    /// so a misconfigured time zone can't crash the caller (e.g. an ingest schedule check).
    /// </summary>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
        {
            // UTC display name / aliases are not valid ids on Linux (or Windows).
            if (timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase)
                || timeZoneId.Equals("Etc/UTC", StringComparison.OrdinalIgnoreCase)
                || timeZoneId.Equals("Coordinated Universal Time", StringComparison.OrdinalIgnoreCase))
                return TimeZoneInfo.Utc;

            // Try converting a Windows id to IANA (and vice-versa) before giving up.
            if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneId, out var ianaId)
                && TryFindTimeZone(ianaId, out var byIana))
                return byIana;
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsId)
                && TryFindTimeZone(windowsId, out var byWindows))
                return byWindows;

            System.Diagnostics.Trace.TraceWarning($"Time zone id '{timeZoneId}' could not be resolved; defaulting to UTC.");
            return TimeZoneInfo.Utc;
        }
    }

    private static bool TryFindTimeZone(string? timeZoneId, out TimeZoneInfo timezone)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return true;
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException) { }
        }
        timezone = TimeZoneInfo.Utc;
        return false;
    }
}
