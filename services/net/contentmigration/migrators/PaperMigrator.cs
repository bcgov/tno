using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.MediaType;
using TNO.API.Areas.Editor.Models.Source;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Extensions;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
/// manages Ingestion of TNO 1.0 'Paper'
/// </summary>
public class PaperMigrator : ContentMigrator<ContentMigrationOptions>, IContentMigrator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public PaperMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrator<ContentMigrationOptions>> logger) : base(api, migratorOptions, options, logger)
    {
    }

    /// <summary>
    /// Creates an Paper SourceContent from a NewsItem
    /// </summary>
    /// <param name="lookups"></param>
    /// <param name="source"></param>
    /// <param name="mediaType"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    public override SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, MediaTypeModel mediaType, ContentType contentType, NewsItem newsItem, string defaultTimeZone)
    {
        // var authors = GetAuthors(lookups.Contributors)
        var publishedOnInDefaultTimeZone = this.GetSourceDateTime(newsItem.GetPublishedDateTime(), defaultTimeZone);
        var publishedOnInUtc = publishedOnInDefaultTimeZone.ToUniversalTime();
        Logger.LogDebug("NewItem.RSN: {rsn}, PublishedDateTime: {publishedDateTime}, ToUtc: {publishedDateTimeInUtc}", newsItem.RSN, publishedOnInDefaultTimeZone, publishedOnInUtc);

        var newsItemTitle = newsItem.GetTitle();
        var sanitizedSummary = TNO.Core.Extensions.StringExtensions.ConvertTextToParagraphs(newsItem.Summary, @"\r\n?|\n|\|");
        var sanitizedBody = TNO.Core.Extensions.StringExtensions.ConvertTextToParagraphs(newsItem.Text, @"\r\n?|\n|\|");
        if (string.IsNullOrEmpty(sanitizedBody)) sanitizedBody = sanitizedSummary;

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            mediaType.Id,
            GetContentHash(source.Code, newsItemTitle, publishedOnInUtc),
            newsItemTitle ?? "",
            sanitizedSummary ?? "",
            sanitizedBody ?? "",
            publishedOnInUtc,
            newsItem.Published)
        {
            Section = newsItem.string2 ?? string.Empty,
            Page = newsItem.string3 ?? string.Empty,
            FilePath = newsItem.FilePath ?? string.Empty,
            Link = newsItem.WebPath ?? string.Empty,
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            ExternalUid = newsItem.WebPath ?? string.Empty
        };

        if (string.IsNullOrEmpty(this.Options.DefaultUserNameForAudit)) throw new System.Configuration.ConfigurationErrorsException("Default Username for ContentMigration has not been configured");
        var auditUser = lookups.Users.FirstOrDefault(u => u.Username == this.Options.DefaultUserNameForAudit) ?? throw new System.Configuration.ConfigurationErrorsException($"Default User for ContentMigration not found : {this.Options.DefaultUserNameForAudit}");
        if (newsItem.Tones?.Any() == true)
        {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMI user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool { Value = (int)t.ToneValue, UserIdentifier = (t.UserRSN == 0 ? null : t.UserRSN.ToString()) });
        }

        // Extract authors from a "delimited" string.  Don't use the source name as an author.
        if (!string.IsNullOrEmpty(newsItem.string5) || !string.IsNullOrEmpty(newsItem.string6))
        {
            var authors = new List<string>();
            if (!string.IsNullOrEmpty(newsItem.string5)) authors.AddRange(ExtractAuthors(newsItem.string5, newsItem.Source));
            if (!string.IsNullOrEmpty(newsItem.string6)) authors.AddRange(ExtractAuthors(newsItem.string6, newsItem.Source));
            content.Authors = authors.Distinct().Select(a => new Author(a));
        }

        if (newsItem.UpdatedOn != null)
        {
            var updatedOnInDefaultTimeZone = this.GetSourceDateTime(newsItem.UpdatedOn.Value, defaultTimeZone);
            var updatedOnInUtc = updatedOnInDefaultTimeZone.ToUniversalTime();
            Logger.LogDebug("NewItem.RSN: {rsn}, UpdatedDateTime: {publishedDateTime}, ToUtc: {publishedDateTimeInUtc}", newsItem.RSN, updatedOnInDefaultTimeZone, updatedOnInUtc);
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? updatedOnInUtc : null;
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory))
        {
            // historic data has some values outside of the enum, just ignore them...
            if (Enum.TryParse(newsItem.EodGroup, out TopicType topicType))
                content.Topics = new[] { new Kafka.Models.Topic { Name = newsItem.EodCategory, TopicType = topicType } };
        }

        // Tags are in the Summary as they are added by an Editor
        if (!string.IsNullOrEmpty(newsItem.Summary))
        {
            // if Tags are found, let the ContentManagement service decide if they are new or not
            content.Tags = ExtractTags(newsItem.Summary)
                .Select(c => new TNO.Kafka.Models.Tag(c.ToUpperInvariant(), ""));
        }

        // map relevant news item properties to actions
        content.Actions = GetActionMappings(newsItem.FrontPageStory, newsItem.WapTopStory, newsItem.Alert,
            newsItem.Commentary, newsItem.CommentaryTimeout);

        // the total "Effort" is stored in the Number2 field as seconds
        if (newsItem.Number2.HasValue && newsItem.Number2 > 0)
        {
            content.TimeTrackings = new[] { new Kafka.Models.TimeTrackingModel {
                Activity = this.Options.DefaultTimeTrackingActivity,
                Effort = (float)Math.Round(Convert.ToDecimal(newsItem.Number2) / 60, 2),
                UserId = auditUser.Id }};
        }

        return content;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter(ContentType contentType)
    {
        string[] excludedContentTypes = new string[] { "video/quicktime", "image/jpeg" };
        switch (contentType)
        {
            case ContentType.PrintContent:
                string[] targetPrintTypes = new string[] { "Newspaper", "Regional" };
                return PredicateBuilder.New<NewsItem>()
                                    .And(ni => targetPrintTypes.Contains(ni.Type!.ToString()))
                                    .And(ni => !excludedContentTypes.Contains(ni.ContentType!.ToString()));
            case ContentType.Internet:
                string[] targetStoryTypes = new string[] { "CP News", "Internet" };
                return PredicateBuilder.New<NewsItem>()
                                    .And(ni => targetStoryTypes.Contains(ni.Type!.ToString()))
                                    .And(ni => !excludedContentTypes.Contains(ni.ContentType!.ToString()));
            default:
                throw new ArgumentException("ContentType must be PrintContent OR Story.");
        }
    }
}
