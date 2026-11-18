using System.Net;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Extensions;

/// <summary>
/// NewsItemExtensions static class, provides extension methods for dictionary objects.
/// </summary>
public static class NewsItemExtensions
{
    /// <summary>
    /// Extract the Published Date and Time
    /// </summary>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    public static DateTime GetPublishedDateTime(this NewsItem newsItem)
    {
        var amalgamatedPublishedDateTime = newsItem.ItemDate ?? newsItem.UpdatedOn ?? DateTime.Now;
        if (newsItem.ItemTime != null)
        {
            amalgamatedPublishedDateTime = new DateTime(amalgamatedPublishedDateTime.Year, amalgamatedPublishedDateTime.Month, amalgamatedPublishedDateTime.Day,
                newsItem.ItemTime.Value.Hour, newsItem.ItemTime.Value.Minute, newsItem.ItemTime.Value.Second);
        }

        return amalgamatedPublishedDateTime;
    }

    /// <summary>
    /// Extract the Title for the NewsItem
    /// </summary>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    public static string GetTitle(this NewsItem newsItem)
    {
        return newsItem.Title != null ? WebUtility.HtmlDecode(newsItem.Title) : string.Empty;
    }

}
