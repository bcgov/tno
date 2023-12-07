namespace TNO.TemplateEngine;

using System.Text.Json;
using TemplateEngine.Models;
using TemplateEngine.Models.Reports;

/// <summary>
/// ReportExtensions static class, provides extension methods for report templates.
/// </summary>
public static class ReportExtensions
{
    #region Methods
    /// <summary>
    /// Get the UTC offset hours for specified 'date' and 'timezoneId'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timezoneId"></param>
    /// <returns></returns>
    public static int GetUtcOffset(DateTime? date = null, string timezoneId = "Pacific Standard Time")
    {
        date ??= System.DateTime.Now;
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return tz.GetUtcOffset(date.Value).Hours;
    }

    /// <summary>
    /// Get today's date and apply the UTC offset for the specified 'timezoneId'.
    /// </summary>
    /// <param name="timezoneId"></param>
    /// <returns></returns>
    public static DateTime GetTodaysDate(string timezoneId = "Pacific Standard Time")
    {
        var now = System.DateTime.Now;
        return now.AddHours(GetUtcOffset(now, timezoneId));
    }

    /// <summary>
    /// Determine if the specified 'content' is audio or video.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsAV(this ContentModel content)
    {
        return content.ContentType == TNO.Entities.ContentType.AudioVideo;
    }

    /// <summary>
    /// Determine if a transcript is available for the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsTranscriptAvailable(this ContentModel content)
    {
        return IsAV(content) && !string.IsNullOrWhiteSpace(content.Body) && content.IsApproved;
    }

    /// <summary>
    /// Get the source code value for the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string GetSourceCode(this ContentModel content)
    {
        return !string.IsNullOrEmpty(content.Source?.Code) ? content.Source.Code : content.OtherSource;
    }

    /// <summary>
    /// Get the summary of the specified content.
    /// Extract the custom values if they have been provided by the report owner.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetSummary(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.ContainsKey(context.OwnerId.Value) ? content.Versions[context.OwnerId.Value].Summary : content.Summary;
    }

    /// <summary>
    /// Get the body or summary of the specified content.
    /// If a transcript is available then get the body.
    /// If the content is audio or video and the transcript has not been approved get the summary.
    /// Otherwise get the body.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetBody(this ContentModel content, ReportEngineContentModel context)
    {
        var contentSummary = content.GetSummary(context);
        var contentBody = context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.Body : content.Body;
        return IsTranscriptAvailable(content) ? contentBody : (IsAV(content) ? contentSummary : contentBody);
    }

    /// <summary>
    /// Highlight a string with the HTML <mark> tag with the query's keywords based on the field type.
    /// Otherwise get the body.
    /// </summary>
    /// <param name="filter">Section filter object</param>
    /// <param name="text">Text to be high lighted</param>
    /// <param name="field">Field to be highlighted (headline, byline or story)</param>
    /// <returns></returns>
    public static string HighlightKeyWords(this FilterModel filter, string text, string field)
    {
        string keywords = "";
        string highlightedText = text;
        if (filter?.Query != null &&
            filter.Query.RootElement.TryGetProperty("query", out JsonElement query) &&
            query.TryGetProperty("bool", out JsonElement queryBool) &&
            queryBool.TryGetProperty("must", out JsonElement queryBoolMust))
        {
            foreach (var c in queryBoolMust.EnumerateArray())
            {
                if (c.TryGetProperty("simple_query_string", out JsonElement simpleQueryString) && simpleQueryString.TryGetProperty("query", out JsonElement simpleQueryStringValue))
                {
                    keywords += simpleQueryStringValue;
                    bool fieldExists = false;
                    if (simpleQueryString.TryGetProperty("fields", out JsonElement simpleQueryStringFields))
                    {
                        foreach (var f in simpleQueryStringFields.EnumerateArray())
                        {
                            if (f.ToString().StartsWith(field))
                            {
                                fieldExists = true;
                            }
                        }
                    }
                    if (!fieldExists)
                    {
                        return text;
                    }
                }
            }
        }

        char[] delimiterChars = { '|', '+', '-' };
        string[] reservedStrings = new string[] { "*", "( and )", "~N", "\"" };
        foreach (var s in reservedStrings)
        {
            keywords = keywords.Replace(s, "");
        }
        foreach (string word in keywords.Split(delimiterChars))
        {
            highlightedText = word != "" ? highlightedText.Replace(word.Trim(), "<mark>" + word.Trim() + "</mark>") : highlightedText;
        }
        return highlightedText;
    }

    /// <summary>
    /// Get the transcription icon for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string GetTranscriptIcon(this ContentModel content)
    {
        return IsTranscriptAvailable(content) ? "▶️📄" : (IsAV(content) ? "▶️" : "");
    }

    /// <summary>
    /// Get the sentiment icon for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetSentimentIcon(this ContentModel content, string format = "{0}")
    {
        var tone = content.TonePools.FirstOrDefault()?.Value;
        var icon = GetSentimentIcon(tone);

        return !String.IsNullOrEmpty(icon) ? String.Format(format, icon) : icon;
    }

    /// <summary>
    /// Get the sentiment icon for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetSentimentIcon(int? tone)
    {
        return tone switch
        {
            0 => "😐",
            <= -3 => "☹️",
            >= 3 => "🙂",
            _ => "",
        };
    }

    /// <summary>
    /// Groups Content by the passed in property
    /// </summary>
    /// <param name="contentList"></param>
    /// <param name="groupBy"></param>
    /// <returns>a dictionary of key values plus the list of grouped Id's</returns>
    public static Dictionary<string, List<long>> GetContentGroupings(this IEnumerable<ContentModel> contentList, string groupBy)
    {
        return contentList
            .GroupBy(g => g.GetContentGroupByPropertyValue(groupBy))
            .ToDictionary(
                g => g.Key,
                g => g.ToList().OrderBy(g => g.Headline).Select(i => i.Id).ToList()
            );
    }

    /// <summary>
    /// returns the value of the property on the ContentModel
    /// </summary>
    /// <param name="content"></param>
    /// <param name="groupBy"></param>
    /// <returns></returns>
    public static string GetContentGroupByPropertyValue(this ContentModel content, string groupBy)
    {
        switch (groupBy)
        {
            case "mediaType":
                return content.MediaType?.Name ?? "";
            case "contentType":
                return content.ContentType.ToString();
            case "byline":
                return content.Byline;
            case "series":
                return content.Series?.Name ?? "";
            case "sentiment":
                return GetSentimentIcon(content.TonePools.FirstOrDefault()?.Value ?? 0);
            case "source":
            default:
                return content.OtherSource;
        }
    }

    /// <summary>
    /// Get the sentiment output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetSentiment(this ContentModel content, ReportEngineContentModel context)
    {
        return (context.Settings.Headline.ShowSentiment ? content.GetSentimentIcon() : "");
    }

    /// <summary>
    /// Get the source name output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetSource(this ContentModel content, ReportEngineContentModel context)
    {
        return context.Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName)
            ? $"{content.Source?.ShortName}"
            : (context.Settings.Headline.ShowSource
                ? content.OtherSource
                : "");
    }

    /// <summary>
    /// Get the headline for the specified content item.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetHeadline(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.ContainsKey(context.OwnerId.Value) ? content.Versions[context.OwnerId.Value].Headline : content.Headline;
    }

    /// <summary>
    /// Get the byline for the specified content item.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetByline(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.ContainsKey(context.OwnerId.Value) ? content.Versions[context.OwnerId.Value].Byline : content.Byline;
    }

    /// <summary>
    /// Get the sentiment output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="utcOffset"></param>
    /// <returns></returns>
    public static string GetPublishedOn(this ContentModel content, ReportEngineContentModel context, int utcOffset = 0)
    {
        return context.Settings.Headline.ShowPublishedOn
                ? $"{content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy}"
                : "";
    }

    /// <summary>
    /// Get the full headline including sentiment, source, publishedOn output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="utcOffset"></param>
    /// <param name="includeLink"></param>
    /// <param name="href"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static string GetFullHeadline(this ContentModel content, ReportEngineContentModel context, int utcOffset = 0, bool includeLink = false, string href = "", string target = "")
    {
        var sentiment = content.GetSentiment(context);
        var headline = content.GetHeadline(context);
        var actualHref = String.IsNullOrWhiteSpace(href) ? $"{context.ViewContentUrl}{content.Id}" : href;
        var actualTarget = String.IsNullOrWhiteSpace(target) ? "" : $" target=\"{target}\"";
        var link = includeLink ? $"<a href=\"{actualHref}\"{actualTarget}>{headline}</a>" : headline;
        var source = content.GetSource(context);
        var publishedOn = content.GetPublishedOn(context, utcOffset);
        return $"{(String.IsNullOrWhiteSpace(sentiment) ? "" : $"{sentiment} - ")}{link}{(String.IsNullOrWhiteSpace(source) ? "" : $" - {source}")}{(String.IsNullOrWhiteSpace(publishedOn) ? "" : $" - {publishedOn}")}";
    }
    #endregion
}
