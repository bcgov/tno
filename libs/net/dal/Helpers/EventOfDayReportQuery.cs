using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TNO.DAL.Helpers;

/// <summary>BC calendar dates and independent copies of historical report filters.</summary>
public static class EventOfDayReportQuery
{
    /// <summary>The time zone the report's calendar days are measured in.</summary>
    public static TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");

    /// <summary>The UTC range [Start, End) covering the BC calendar 'date'.</summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static (DateTime Start, DateTime End) GetDayRange(DateOnly date) => (
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), TimeZone),
        TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeZone));

    /// <summary>The BC calendar date of a UTC instant.</summary>
    /// <param name="utc"></param>
    /// <returns></returns>
    public static DateOnly GetDate(DateTime utc) => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone));

    /// <summary>
    /// The publication time for a report generated after the fact: the BC time of day the report
    /// usually goes out (taken from 'templateUtc', a prior instance), otherwise the end of the day.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="templateUtc"></param>
    /// <returns></returns>
    public static DateTime GetPublishedOn(DateOnly date, DateTime? templateUtc)
    {
        var (start, end) = GetDayRange(date);
        if (templateUtc.HasValue)
        {
            var template = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(templateUtc.Value, DateTimeKind.Utc), TimeZone);
            var local = date.ToDateTime(TimeOnly.FromDateTime(template));
            if (TimeZone.IsInvalidTime(local)) local = local.AddHours(1);
            var utc = TimeZoneInfo.ConvertTimeToUtc(local, TimeZone);
            if (utc >= start && utc < end) return utc;
        }
        return end.AddSeconds(-1);
    }

    /// <summary>
    /// An independent copy of the filter's query as it would have run at 'publishedOn': relative dates are
    /// anchored there, nothing after that day is included, and every hit is fetched in ID order.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    public static JsonObject Create(JsonDocument filter, DateTime publishedOn)
    {
        var query = JsonNode.Parse(filter.RootElement.GetRawText())!.AsObject();
        var anchor = DateTime.SpecifyKind(publishedOn, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
        AnchorDates(query, anchor);
        var (_, end) = GetDayRange(GetDate(publishedOn));
        // Old filters often have only a lower bound. Never include days after this report.
        query["query"] = new JsonObject
        {
            ["bool"] = new JsonObject
            {
                ["must"] = new JsonArray(query["query"]?.DeepClone() ?? new JsonObject { ["match_all"] = new JsonObject() }),
                ["filter"] = new JsonArray(
                    new JsonObject { ["range"] = new JsonObject { ["publishedOn"] = new JsonObject { ["lt"] = end.ToString("O") } } },
                    new JsonObject { ["term"] = new JsonObject { ["isPrivate"] = false } })
            }
        };
        query.Remove("from");
        query.Remove("_source");
        query.Remove("search_after");
        query["size"] = 1000;
        query["sort"] = new JsonArray(new JsonObject { ["id"] = "asc" });
        query["track_total_hits"] = true;
        return query;
    }

    private static void AnchorDates(JsonNode node, string anchor)
    {
        if (node is JsonObject obj)
        {
            foreach (var pair in obj.ToArray())
            {
                // Only replace date-math range operands, never text searched for the word "now".
                if (pair.Key is "gte" or "gt" or "lte" or "lt" &&
                    pair.Value is JsonValue value && value.TryGetValue<string>(out var text) &&
                    (text == "now" || text.StartsWith("now-") || text.StartsWith("now+") || text.StartsWith("now/")))
                    obj[pair.Key] = text == "now" ? anchor : anchor + "||" + text[3..];
                else if (pair.Value != null) AnchorDates(pair.Value, anchor);
            }
        }
        else if (node is JsonArray array)
            foreach (var child in array.Where(n => n != null)) AnchorDates(child!, anchor);
    }
}
