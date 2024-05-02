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
        var value = content.TonePools.FirstOrDefault()?.Value;
        var icon = GetSentimentIcon(value);

        return !String.IsNullOrEmpty(icon) ? String.Format(format, icon) : icon;
    }

    /// <summary>
    /// Get the sentiment icon for the content.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="showSentimentValue"></param>
    /// <returns></returns>
    public static string GetSentimentIcon(int? value, bool showSentimentValue = false)
    {
        return value switch
        {
            0 => $"<span style=\"color: #FFC107;\">😐{(showSentimentValue ? $" {value}</span>" : "")}",
            <= -3 => $"<span style=\"color: #DC3545;\">☹️{(showSentimentValue ? $" {value}</span>" : "")}",
            < 0 => $"<span style=\"color: #FFC107;\">😐{(showSentimentValue ? $" {value}</span>" : "")}",
            >= 3 => $"<span style=\"color: #20C997;\">🙂{(showSentimentValue ? $" {value}</span>" : "")}",
            > 0 => $"<span style=\"color: #FFC107;\">😐{(showSentimentValue ? $" {value}</span>" : "")}",
            _ => "",
        };
    }

    /// <summary>
    /// Get the sentiment icon for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="showSentimentValue"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetSentimentImage(this ContentModel content, ReportEngineContentModel context, bool showSentimentValue = false, string format = "{0}")
    {
        var value = content.TonePools.FirstOrDefault()?.Value;
        var icon = context.GetSentimentImage(value, showSentimentValue);

        return !String.IsNullOrEmpty(icon) ? String.Format(format, icon) : icon;
    }

    /// <summary>
    /// Get the sentiment icon for the content.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <param name="showSentimentValue"></param>
    /// <returns></returns>
    public static string GetSentimentImage(this ReportEngineContentModel context, int? value, bool showSentimentValue = false)
    {
        return value switch
        {
            0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-neutral@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #FFC107;font-size: 12px;\">{value}</span>" : "")}",
            <= -3 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-negative@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #DC3545;font-size: 12px;\">{value}</span>" : "")}",
            < 0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-neutral@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #FFC107;font-size: 12px;\">{value}</span>" : "")}",
            >= 3 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-positive@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #20C997;font-size: 12px;\">{value}</span>" : "")}",
            > 0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-neutral@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #FFC107;font-size: 12px;\">{value}</span>" : "")}",
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
        return groupBy switch
        {
            "mediaType" => content.MediaType?.Name ?? "",
            "contentType" => content.ContentType.ToString(),
            "topicType" => content.Topics.FirstOrDefault()?.TopicType.ToString() ?? "",
            "topicName" => content.Topics.FirstOrDefault()?.Name ?? "",
            "byline" => content.Byline,
            "series" => content.Series?.Name ?? "",
            "sentiment" => GetSentimentIcon(content.TonePools.FirstOrDefault()?.Value ?? 0),
            _ => content.OtherSource,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentList"></param>
    /// <returns></returns>
    public static Dictionary<string, int> GetTotalScoresByTopicType(this IEnumerable<ContentModel> contentList) {
        
        return contentList
            .Where(a => 
                a.Topics.Any() // must have at least ONE Topic set
                && a.Topics.All(a => a.Name != "Not Applicable" && !string.IsNullOrEmpty(a.Name)) // Name must not "Not Applicable" OR be empty
                && a.Topics.All(t => t.Score > 0) // must have a Topic Score > ZERO
            )
            .GroupBy(g => g.GetContentGroupByPropertyValue("topicType"))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(s => s.Topics.First().Score)
            );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentList"></param>
    /// <returns></returns>
    public static Dictionary<string, Dictionary<string,int>> GetTopicScoresByTopicTypeAndName(this IEnumerable<ContentModel> contentList) {
        return contentList
            .Where(a => 
                a.Topics.Any() // must have at least ONE Topic set
                && a.Topics.All(a => a.Name != "Not Applicable" && !string.IsNullOrEmpty(a.Name)) // Name must not "Not Applicable" OR be empty
                && a.Topics.All(t => t.Score > 0) // must have a Topic Score > ZERO
            )
            .GroupBy(g => g.GetContentGroupByPropertyValue("topicType"))
            .ToDictionary(
                g => g.Key,
                g => g.ToList().GroupBy(g => g.GetContentGroupByPropertyValue("topicName"))
                    .ToDictionary(
                        gg => gg.Key,
                        gg => gg.Sum(s => s.Topics.FirstOrDefault()?.Score ?? 0))
            );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aggregations"></param>
    /// <returns></returns>
    public static Dictionary<string, int> GetTotalScoresByTopicType(this Dictionary<string, AggregationRootModel> aggregations) {
        return aggregations.First().Value.ChildAggregation.Buckets
            .ToDictionary(
                g => g.Key,
                g => (int)(g.AggregationSum?.Value ?? 0)
            );
    }

    /// <summary>
    /// Helper converts aggregated result from ElasticSearch to something easier to work with
    /// </summary>
    /// <param name="aggregations"></param>
    /// <returns></returns>
    public static Dictionary<string, Dictionary<string,int>> GetTopicScoresByTopicTypeAndName(this Dictionary<string, AggregationRootModel> aggregations) {
        return aggregations.First().Value.ChildAggregation.Buckets
            .ToDictionary(
                g => g.Key,
                g => g.ChildAggregation!.Buckets.ToDictionary(
                    gg => gg.Key,
                    gg => (int)(gg.AggregationSum?.Value ?? 0)
                )
            );
    }

    /// <summary>
    /// Get the sentiment output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="showSentimentValue"></param>
    /// <returns></returns>
    public static string GetSentiment(this ContentModel content, ReportEngineContentModel context, bool showSentimentValue = false)
    {
        return context.Settings.Headline.ShowSentiment ? content.GetSentimentImage(context, showSentimentValue) : "";
    }

    /// <summary>
    /// Get the source name output.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetSource(this ContentModel content, ReportEngineContentModel context)
    {
        var name = context.Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName)
            ? $"{content.Source?.ShortName}"
            : "";

        var source = (context.Settings.Headline.ShowSource
                ? content.OtherSource.ToUpperInvariant()
                : "");

        return $"{source}{(!String.IsNullOrWhiteSpace(name) ? $" - {name}" : "")}";
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
        if (content.IsAV())
            return context.Settings.Headline.ShowPublishedOn
                    ? $"{content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy H:mm tt}"
                    : "";
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
    /// <param name="showSentimentValue"></param>
    /// <returns></returns>
    public static string GetFullHeadline(this ContentModel content, ReportEngineContentModel context, int utcOffset = 0, bool includeLink = false, string href = "", string target = "", bool showSentimentValue = false)
    {
        var byline = context.Settings.Headline.ShowByline && !String.IsNullOrWhiteSpace(content.Byline) ? $" - {content.Byline}" : "";
        var sentiment = context.Settings.Headline.ShowSentiment ? content.GetSentiment(context, showSentimentValue) : "";
        var headline = content.GetHeadline(context);
        var headlineValue = !String.IsNullOrWhiteSpace(headline) ? headline : "NO HEADLINE";
        var actualHref = String.IsNullOrWhiteSpace(href) ? $"{context.ViewContentUrl}{content.Id}" : href;
        var actualTarget = String.IsNullOrWhiteSpace(target) ? "" : $" target=\"{target}\"";
        var link = includeLink ? $"<a href=\"{actualHref}\"{actualTarget}>{headlineValue}</a>" : headlineValue;
        var source = content.GetSource(context);
        var publishedOn = context.Settings.Headline.ShowPublishedOn ? content.GetPublishedOn(context, utcOffset) : "";
        return $"{(String.IsNullOrWhiteSpace(sentiment) ? "" : $"{sentiment} - ")}{link}{byline}{(String.IsNullOrWhiteSpace(source) ? "" : $" - {source}")}{(String.IsNullOrWhiteSpace(publishedOn) ? "" : $" - {publishedOn}")}";
    }

    /// <summary>
    /// searches list of `knownValues` to get index of corresponding color in `colorLookup`
    /// throws an exception of knownValues and colorLookup are not the same length
    /// returns an inline css color snippet if a match is found
    /// </summary>
    /// <param name="targetValue">what value to search for</param>
    /// <param name="knownValues">what are the values we want to have colors for</param>
    /// <param name="colorLookup">what are the corresponding colors for the known values</param>
    /// <returns></returns>
    public static string GetColorFromName(string targetValue, string[] knownValues, string[] colorLookup)
    {
        if (knownValues.Length != colorLookup.Length)
            throw new ArgumentException("Array length mismatch.  Each known value must have a corresponding color");
        int indexOfValue = knownValues.ToList().FindIndex(x => x.ToLower() == targetValue.ToLower());
        if (indexOfValue >= 0)
        {
            return $"color:{colorLookup[indexOfValue]}";
        }
        else return string.Empty;
    }

    /// <summary>
    /// returns a the sum of all child Aggregations
    /// </summary>
    /// <param name="aggregations"></param>
    /// <returns></returns>
    public static double GetSumOfAggregations(Dictionary<string, AggregationRootModel> aggregations)
    {
        double returnVal = 0;

        foreach (KeyValuePair<string, AggregationRootModel> aggregation in aggregations)
        {
            double childAggregationSum = 0.0;
            foreach (var bucket in aggregation.Value.ChildAggregation.Buckets)
            {
                if (bucket.AggregationSum != null) childAggregationSum += bucket.AggregationSum.Value;
            }
            returnVal += childAggregationSum;
        }

        return returnVal;
    }
    #endregion
}
