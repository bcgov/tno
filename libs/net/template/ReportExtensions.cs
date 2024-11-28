namespace TNO.TemplateEngine;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TemplateEngine.Models;
using TemplateEngine.Models.Reports;
using TNO.Elastic;

/// <summary>
/// ReportExtensions static class, provides extension methods for report templates.
/// </summary>
public static partial class ReportExtensions
{
    #region Variables
    [GeneratedRegex("<img[^>]*>")]
    private static partial Regex StripHtmlImagesRegex();
    #endregion

    #region Methods
    /// <summary>
    /// Get the UTC offset hours for specified 'date' and 'timezoneId'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timezoneId"></param>
    /// <returns></returns>
    public static int GetUtcOffset(DateTime? date = null, string timezoneId = "Pacific Standard Time")
    {
        date ??= DateTime.Now;
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
        var now = DateTime.Now;
        return now.AddHours(GetUtcOffset(now, timezoneId));
    }

    /// <summary>
    /// Determine if the specified 'content' is audio or video.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsAV(this ContentModel content)
    {
        return content.ContentType == Entities.ContentType.AudioVideo;
    }

    /// <summary>
    /// Determine if a transcript is available for the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsTranscriptAvailable(this ContentModel content)
    {
        return content.IsAV() && !string.IsNullOrWhiteSpace(content.Body) && content.IsApproved;
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
        return context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.Summary : content.Summary;
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
        return content.IsTranscriptAvailable() ? contentBody : content.IsAV() ? contentSummary : contentBody;
    }

    /// <summary>
    /// Filter out html <img> blocks from the string
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string StripHtmlImages(this string html)
    {
        return StripHtmlImagesRegex().Replace(html, "");
    }

    /// <summary>
    /// Extract keywords from filter and mark them in the content summary and body.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="content"></param>
    public static void MarkKeywords(this TNO.TemplateEngine.Models.Reports.ReportSectionModel section, TNO.TemplateEngine.Models.ContentModel content)
    {
        // No need to mark keywords if there is no filter with keywords to search for.
        if (section.Filter == null) return;
        var settings = section.Filter.Settings;
        var search = settings?.Search;
        if (String.IsNullOrWhiteSpace(search)) return;

        try
        {
            var keywords = settings?.Search?.ExtractKeywords(settings.QueryType) ?? [];
            var headline = content.Headline.MarkKeywords(keywords);
            var summary = content.Summary.MarkKeywords(keywords);
            var body = content.Body.MarkKeywords(keywords);
            var byline = content.Byline.MarkKeywords(keywords);

            content.Headline = headline;
            content.Summary = summary;
            content.Body = body;
            content.Byline = byline;
        }
        catch
        {
            // Ignore errors for now
        }
    }

    /// <summary>
    /// Get the transcription icon for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string GetTranscriptIcon(this ContentModel content)
    {
        return content.IsTranscriptAvailable() ? "▶️📄" : content.IsAV() ? "▶️" : "";
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

        return !string.IsNullOrEmpty(icon) ? string.Format(format, icon) : icon;
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
            < 0 => $"<span style=\"color: #DC3545;\">☹️{(showSentimentValue ? $" {value}</span>" : "")}",
            > 0 => $"<span style=\"color: #20C997;\">🙂{(showSentimentValue ? $" {value}</span>" : "")}",
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

        return !string.IsNullOrEmpty(icon) ? string.Format(format, icon) : icon;
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
            0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-neutral@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #f1c02d;font-size: 12px;\">{value}</span>" : "")}",
            < 0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-negative@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #eb8585;font-size: 12px;\">{value}</span>" : "")}",
            > 0 => $"<img height=\"16\" width=\"16\" style=\"height: 16px; width: 16px;\" src=\"{context.SubscriberAppUrl}assets/reports/face-positive@2x.png\" alt=\"{value}\" />{(showSentimentValue ? $" <span style=\"color: #59e9be;font-size: 12px;\">{value}</span>" : "")}",
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
            "byline" => content.Byline.Replace("<mark>", "").Replace("</mark>", ""),
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
    public static Dictionary<string, int> GetTotalScoresByTopicType(this IEnumerable<ContentModel> contentList)
    {

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
    public static Dictionary<string, Dictionary<string, int>> GetTopicScoresByTopicTypeAndName(this IEnumerable<ContentModel> contentList)
    {
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
    public static Dictionary<string, int> GetTotalScoresByTopicType(this Dictionary<string, AggregationRootModel> aggregations)
    {
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
    public static Dictionary<string, Dictionary<string, int>> GetTopicScoresByTopicTypeAndName(this Dictionary<string, AggregationRootModel> aggregations)
    {
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
        var name = context.Settings.Headline.ShowShortName && !string.IsNullOrEmpty(content.Source?.ShortName)
            ? $"{content.Source?.ShortName}"
            : "";

        var source = "";
        if (context.Settings.Headline.ShowSource)
        {
            source = !string.IsNullOrEmpty(content?.Source?.Name)
                ? content.Source.Name
                : content?.OtherSource;
        }
        return $"{source}{(!string.IsNullOrWhiteSpace(name) ? $" - {name}" : "")}";
    }

    /// <summary>
    /// Get the headline for the specified content item.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetHeadline(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.Headline : content.Headline;
    }

    /// <summary>
    /// Get the byline for the specified content item.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetByline(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.Byline : content.Byline;
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
        var byline = context.Settings.Headline.ShowByline && !string.IsNullOrWhiteSpace(content.Byline) ? $" - {content.Byline}" : "";
        var sentiment = context.Settings.Headline.ShowSentiment ? content.GetSentiment(context, showSentimentValue) : "";
        var headline = content.GetHeadline(context);
        var headlineValue = !string.IsNullOrWhiteSpace(headline) ? headline : "NO HEADLINE";
        var actualHref = string.IsNullOrWhiteSpace(href) ? $"{context.ViewContentUrl}{content.Id}" : href;
        var actualTarget = string.IsNullOrWhiteSpace(target) ? "" : $" target=\"{target}\"";
        var link = includeLink ? $"<a href=\"{actualHref}\"{actualTarget}>{headlineValue}</a>" : headlineValue;
        var source = content.GetSource(context);
        var publishedOn = context.Settings.Headline.ShowPublishedOn ? content.GetPublishedOn(context, utcOffset) : "";
        return $"{(string.IsNullOrWhiteSpace(sentiment) ? "" : $"{sentiment} - ")}{link}{byline}{(string.IsNullOrWhiteSpace(source) ? "" : $" - {source}")}{(string.IsNullOrWhiteSpace(publishedOn) ? "" : $" - {publishedOn}")}";
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
    public static string? GetColorFromName(string targetValue, string[] knownValues, string[]? colorLookup)
    {
        if (colorLookup == null || colorLookup.Length == 0) return null;
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

    /// <summary>
    /// Get the Source URL for the specified content item.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetSourceUrl(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.SourceUrl : content.SourceUrl;
    }

    /// <summary>
    /// Determine if the specified 'content' is private.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsPrivate(this ContentModel content, ReportEngineContentModel context)
    {
        return context.OwnerId.HasValue && content.Versions.TryGetValue(context.OwnerId.Value, out Entities.Models.ContentVersion? value) ? value.IsPrivate : content.IsPrivate;
    }

    /// Group the content to separate it for the chart.
    /// This is used to separate datasets and to identify labels.
    /// </summary>
    /// <param name="groupBy"></param>
    /// <param name="content"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    public static IEnumerable<IGrouping<string, ContentModel>> GroupContent(
        string groupBy,
        IEnumerable<ContentModel> content,
        Dictionary<string, ReportSectionModel> sections,
        API.Models.Settings.ChartSectionSettingsModel? settings = null)
    {
        if (groupBy == "topicType")
        {
            // Extract all topic types from content.
            var topicTypes = content.SelectMany(c => c.Topics.Select(t => t.TopicType)).Distinct();
            var topicTypesDict = new Dictionary<string, IEnumerable<ContentModel>>();
            foreach (var type in topicTypes.OrderBy(t => t))
            {
                topicTypesDict.Add(type.ToString(), content.Where(c => c.Topics.Any(t => t.TopicType == type)));
            }
            var spread = topicTypesDict.SelectMany(d => d.Value.Select(v => new KeyValuePair<string, ContentModel>(d.Key, v)));
            return spread.GroupBy(s => s.Key, s => s.Value);
        }
        else if (groupBy == "topicName")
        {
            // Extract all topic names from content.
            var topicNames = content.SelectMany(c => c.Topics.Select(t => t.Name)).Distinct();
            var topicNamesDict = new Dictionary<string, IEnumerable<ContentModel>>();
            foreach (var name in topicNames.OrderBy(t => t))
            {
                topicNamesDict.Add(name.ToString(), content.Where(c => c.Topics.Any(t => t.Name == name)));
            }
            var spread = topicNamesDict.SelectMany(d => d.Value.Select(v => new KeyValuePair<string, ContentModel>(d.Key, v)));
            return spread.GroupBy(s => s.Key, s => s.Value);
        }
        else if (groupBy == "reportSection")
        {
            // Extract report sections and then group content into the sections.
            var sectionsKeys = sections.OrderBy(s => s.Value.SortOrder).Select(s => (s.Value.Name, s.Value.Settings.Label));
            var sectionDict = new Dictionary<string, IEnumerable<ContentModel>>();
            foreach (var (Name, Label) in sectionsKeys)
            {
                sectionDict.Add(Name, content.Where(c => c.SectionName == Name));
            }
            var spread = sectionDict.SelectMany(d => d.Value.Select(v => new KeyValuePair<string, ContentModel>(d.Key, v)));
            return spread.GroupBy(s => s.Key, s => s.Value);
        }

        var excludeEmptyValues = settings?.ExcludeEmptyValues ?? false;
        var isSentiment = settings?.DatasetValue == "sentiment";

        var groups = groupBy switch
        {
            "mediaType" => content.GroupBy(c => c.MediaType?.Name ?? "Other").OrderBy(group => group.Key),
            "contentType" => content.GroupBy(c => c.ContentType.ToString()).OrderBy(group => group.Key),
            "byline" => content.GroupBy(c => string.IsNullOrWhiteSpace(c.Byline) ? "Unknown" : c.Byline.Replace("<mark>", "").Replace("</mark>", "")).Where((g) => !excludeEmptyValues || g.Key != "Unknown").OrderBy(group => group.Key),
            "series" => content.GroupBy(c => c.Series?.Name ?? c.OtherSeries ?? "None").Where((g) => !excludeEmptyValues || g.Key != "None").OrderBy(group => group.Key),
            "sentiment" => content.GroupBy(c => GetSentimentValue(c)?.ToString() ?? "None").Where((v) => !excludeEmptyValues || v.Key != "None").OrderByDescending(group => group.Key),
            "sentimentSimple" => content.GroupBy(c => GetSentimentRating(c) ?? "None").Where((v) => !excludeEmptyValues || v.Key != "None").OrderBy(group => group.Key),
            "source" => content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
            "dayMonthYear" => content.GroupBy(c => $"{c.PublishedOn:dd-MM-yyyy}").OrderBy(group => group.Key),
            "monthDay" => content.GroupBy(c => $"{c.PublishedOn:MMM-dd}").OrderBy(group => group.Key),
            "monthYear" => content.GroupBy(c => $"{c.PublishedOn:MM-yyyy}").OrderBy(group => group.Key),
            "year" => content.GroupBy(c => $"{c.PublishedOn:yyyy}").OrderBy(group => group.Key),
            _ => content.GroupBy(c => isSentiment ? "Average Sentiment" : "Story Count"),
        };

        // If excluding empty values is on, then remove any groups that only contain content without a value.
        return groups.Where(g => !excludeEmptyValues || !isSentiment || g.Any(c => GetSentimentValue(c) != null));
    }

    /// <summary>
    /// Determine the sentiment value based on the 'datasetName'.
    /// This is used to determine the minBarLength.
    /// </summary>
    /// <param name="datasetName"></param>
    /// <returns></returns>
    public static int? GetSentimentSwitch(string datasetName)
    {
        if (int.TryParse(datasetName, out int value))
            return value;
        else
        {
            return datasetName switch
            {
                "Positive" => 1,
                "Neutral" => 0,
                "Negative" => -1,
                _ => null,
            };
        }
    }

    /// <summary>
    /// Provides a javascript lambda function that will apply the correct colour in the sequence based on the value in the dataset.
    /// </summary>
    /// <param name="colors"></param>
    /// <returns></returns>
    public static string GetSentimentColorScript(string[]? colors)
    {
        var positive = colors?.Length > 0 ? colors[0] : "green";
        var neutral = colors?.Length > 1 ? colors[1] : "gold";
        var negative = colors?.Length > 2 ? colors[2] : "red";

        return "\"(ctx,options) => {{ " +
            "const index = ctx.dataIndex; " +
            "const value = ctx.dataset.data[index]; " +
            $"if (typeof value === 'number') return value === null ? null : value > 0 ? '{positive}' : value === 0 ? '{neutral}' : '{negative}'; " +
            "}}\"";
    }

    /// <summary>
    /// Extract the color for the specified dataset, or base the color the sentiment value.
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="index">If -1 it will return all colours in the array.</param>
    /// <param name="dataset"></param>
    /// <param name="datasetName"></param>
    /// <param name="datasetValueProp"></param>
    /// <returns></returns>
    public static string? GetColors(string[]? colors, int index, string dataset, string datasetName, string datasetValueProp)
    {
        if (dataset == "" && datasetValueProp == "sentiment")
        {
            return GetSentimentColorScript(colors);
        }

        if (dataset == "sentiment" || dataset == "sentimentSimple")
        {
            // A chart with a dataset for each sentiment should choose the color associated with the sentiment value.
            if (colors == null || colors.Length < 3)
                colors = new[] { "green", "gold", "red" };

            if (int.TryParse(datasetName, out int value))
                return $"[\"{(value > 0 ? colors[0] : value == 0 ? colors[1] : colors[2])}\"]";
            else
            {
                return $"[\"{datasetName switch
                {
                    "Positive" => colors[0],
                    "Neutral" => colors[1],
                    "Negative" => colors[2],
                    _ => colors[0],
                }}\"]";
            }
        }

        // This will return a colour for each value in the dataset.
        if (index < 0 || colors == null) return colors == null ? "null" : $"[{String.Join(",", colors.Select(c => $"\"{c}\""))}]";

        // Use the index of the dataset to pick the colour.
        index = index >= 0 ? index : 0;
        var length = colors.Length;
        if (length == 0) return "null";
        var position = index < length ? index : index % length;
        return $"[\"{colors[position]}\"]";
    }

    /// <summary>
    /// Extract the labels for each grouping for the chart.
    /// Use the GroupContent() method to group first.
    /// </summary>
    /// <param name="datasets"></param>
    /// <param name="settings"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    public static string[] GetLabels(IEnumerable<IGrouping<string, ContentModel>> datasets, API.Models.Settings.ChartSectionSettingsModel settings, Dictionary<string, ReportSectionModel> sections)
    {
        return datasets.Select(ds => GetLabel(ds, settings, sections)).ToArray();
    }

    /// <summary>
    /// Extract the label for the specified dataset.
    /// Use the GroupContent() method to group first.
    /// </summary>
    /// <param name="dataset"></param>
    /// <param name="settings"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    public static string GetLabel(IGrouping<string, ContentModel> dataset, API.Models.Settings.ChartSectionSettingsModel settings, Dictionary<string, ReportSectionModel> sections)
    {
        var hasSection = sections.TryGetValue(dataset.Key, out ReportSectionModel? section);
        if (hasSection && section != null && (settings.Dataset == "reportSection" || settings.GroupBy == "reportSection"))
            return section.Settings.Label ?? "Other";

        return dataset.Key;
    }

    /// <summary>
    /// Get the sentiment value for the specified 'content'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static int? GetSentimentValue(ContentModel content)
    {
        return content.TonePools.FirstOrDefault()?.Value;
    }

    /// <summary>
    /// Get the sentiment rating for the specified 'content'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string? GetSentimentRating(ContentModel content)
    {
        var value = GetSentimentValue(content);
        if (value < 0) return "Negative";
        if (value == 0) return "Neutral";
        if (value > 0) return "Positive";
        return null;
    }

    /// <summary>
    /// Align the content with the labels array so that each label has a value assigned to it.
    /// </summary>
    /// <param name="datasetValue"></param>
    /// <param name="groupBy"></param>
    /// <param name="content"></param>
    /// <param name="labels"></param>
    /// <returns></returns>
    public static int?[] ExtractDatasetValues(
        string datasetValue,
        string groupBy,
        IEnumerable<ContentModel> content,
        string[] labels)
    {
        var values = new List<int?>();
        foreach (var label in labels)
        {
            var items = groupBy switch
            {
                "mediaType" => content.Where(c => (c.MediaType?.Name ?? "Other") == label),
                "contentType" => content.Where(c => c.ContentType.ToString() == label),
                "byline" => content.Where(c => (string.IsNullOrWhiteSpace(c.Byline) ? "Unknown" : c.Byline.Replace("<mark>", "").Replace("</mark>", "")) == label),
                "series" => content.Where(c => (c.Series?.Name ?? "None") == label),
                "sentiment" => content.Where(c => (GetSentimentValue(c)?.ToString() ?? "None") == label),
                "sentimentSimple" => content.Where(c => (GetSentimentRating(c) ?? "None") == label),
                "source" => content.Where(c => c.OtherSource == label),
                "dayMonthYear" => content.Where(c => $"{c.PublishedOn:dd-MM-yyyy}" == label),
                "monthDay" => content.Where(c => $"{c.PublishedOn:MMM-dd}" == label),
                "monthYear" => content.Where(c => $"{c.PublishedOn:MM-yyyy}" == label),
                "year" => content.Where(c => $"{c.PublishedOn:yyyy}" == label),
                "reportSection" => content.Where(c => c.SectionLabel == label),
                _ => content,
            };

            if (datasetValue == "sentiment")
            {
                var avg = items.Any() ? items.Select(c => GetSentimentValue(c)).Where(v => v != null).Average(v => v) : null;
                if (avg.HasValue)
                    values.Add((int)Math.Round(avg.Value, MidpointRounding.AwayFromZero));
                else values.Add(null);
            }
            else
            {
                values.Add(items.Count());
            }
        }
        return values.ToArray();
    }
    #endregion
}
