namespace TNO.TemplateEngine;

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
    /// Get the body or summary of the specified content.
    /// If a transcript is available then get the body.
    /// If the content is audio or video get the summary.
    /// Otherwise get the body.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string GetBody(this ContentModel content)
    {
        return IsTranscriptAvailable(content) ? content.Body : (IsAV(content) ? content.Summary : content.Body);
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
    public static string GetSentimentIcon(this ContentModel content, string format)
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
            case "product":
                return content.Product?.Name ?? "";
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
    #endregion
}
