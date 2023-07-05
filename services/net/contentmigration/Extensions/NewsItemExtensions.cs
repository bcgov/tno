using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Extensions;

/// <summary>
/// DictionaryExtensions static class, provides extension methods for dictionary objects.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Extract the Published Date and Time
    /// </summary>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    public static DateTime GetPublishedDateTime(this NewsItem newsItem)
    {
        var amalgamatedPublishedDateTime = newsItem.ItemDateTime ?? DateTime.UtcNow;
        if (newsItem.ItemDateTime != null)
        {
            amalgamatedPublishedDateTime = new DateTime(amalgamatedPublishedDateTime.Year, amalgamatedPublishedDateTime.Month, amalgamatedPublishedDateTime.Day,
                newsItem.ItemDateTime.Value.Hour, newsItem.ItemDateTime.Value.Minute, newsItem.ItemDateTime.Value.Second);
        }

        return amalgamatedPublishedDateTime;
    }

}
