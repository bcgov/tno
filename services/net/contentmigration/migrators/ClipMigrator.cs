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
/// manages Ingestion of TNO 1.0 'AudioVideo'
/// </summary>
public class ClipMigrator : ContentMigrator<ContentMigrationOptions>, IContentMigrator
{
    #region Constructors
    /// <summary>
    ///
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ClipMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrator<ContentMigrationOptions>> logger) : base(api, migratorOptions, options, logger)
    {
    }
    #endregion

    /// <summary>
    /// Creates an Clip SourceContent from a NewsItem
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

        // KGM: Would be nice to do this, but I don't think we could map a clip back to a schedule so there may be duplicates here
        // var referenceUid = $"{schedule.Name}:{schedule.Id}-{publishedOn:yyyy-MM-dd-hh-mm-ss}";
        var publishedOnInDefaultTimeZone = this.GetSourceDateTime(newsItem.GetPublishedDateTime(), defaultTimeZone);
        var publishedOnInUtc = publishedOnInDefaultTimeZone.ToUniversalTime();
        Logger.LogDebug("NewItem.RSN: {rsn}, PublishedDateTime: {publishedDateTime}, ToUtc: {publishedDateTimeInUtc}", newsItem.RSN, publishedOnInDefaultTimeZone, publishedOnInUtc);

        var sanitizedSummary = TNO.Core.Extensions.StringExtensions.ConvertTextToParagraphs(newsItem.Summary, @"\r\n?|\n|\|");
        var sanitizedBody = TNO.Core.Extensions.StringExtensions.ConvertTextToParagraphs(newsItem.Transcript, @"\r\n?|\n|\|");

        // Scrum/Events place their transcript in the summary.
        if (String.IsNullOrWhiteSpace(sanitizedBody) && mediaType.Name == "Events") sanitizedBody = sanitizedSummary;

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            mediaType.Id,
            newsItem.RSN.ToString(),
            newsItem.GetTitle(),
            sanitizedSummary ?? string.Empty,
            sanitizedBody ?? string.Empty,
            publishedOnInUtc,
            newsItem.Published)
        {
            HashUid = Runners.BaseService.GetContentHash(source.Code, newsItem.GetTitle(), publishedOnInUtc),
            ExternalUid = newsItem.WebPath ?? string.Empty,
            Link = newsItem.WebPath ?? string.Empty,
            FilePath = newsItem.FilePath ?? string.Empty,
            Language = string.Empty, // TODO: Need to extract this from the ingest, or determine it after transcription.
        };

        if (string.IsNullOrEmpty(this.Options.DefaultUserNameForAudit)) throw new System.Configuration.ConfigurationErrorsException("Default Username for ContentMigration has not been configured");
        var auditUser = lookups.Users.FirstOrDefault(u => u.Username == this.Options.DefaultUserNameForAudit) ?? throw new System.Configuration.ConfigurationErrorsException($"Default User for ContentMigration not found : {this.Options.DefaultUserNameForAudit}");

        // newsItem.string5 and newsItem.string5 both seem to be the "Show/Program"
        if (newsItem.string5 != null)
        {
            content.Series = newsItem.string5;
        }

        if (newsItem.UpdatedOn != null)
        {
            var updatedOnInDefaultTimeZone = this.GetSourceDateTime(newsItem.UpdatedOn.Value, defaultTimeZone);
            var updatedOnInUtc = updatedOnInDefaultTimeZone.ToUniversalTime();
            Logger.LogDebug("NewItem.RSN: {rsn}, UpdatedDateTime: {publishedDateTime}, ToUtc: {publishedDateTimeInUtc}", newsItem.RSN, updatedOnInDefaultTimeZone, updatedOnInUtc);
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? updatedOnInUtc : null;
        }

        if (newsItem.Tones?.Any() == true)
        {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMI user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool { Value = (int)t.ToneValue, UserIdentifier = t.UserRSN == 0 ? null : t.UserRSN.ToString() });
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory))
        {
            // historic data has some values outside of the enum, just ignore them...
            if (Enum.TryParse(newsItem.EodGroup, out TopicType topicType))
                content.Topics = new[] { new Kafka.Models.Topic(newsItem.EodCategory, topicType, newsItem.Topics.FirstOrDefault()?.Score) };
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
                Effort = (float)Math.Round(Convert.ToDecimal(newsItem.Number2 / 60), 2),
                UserId = auditUser.Id }};
        }

        return content;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<T, bool>> GetBaseFilter<T>(ContentType contentType)
    {
        string[] targetTypes = new string[] { "Radio News", "TV News", "Talk Radio", "Scrum", "CC News" };
        return PredicateBuilder.New<T>()
                .And(ni => targetTypes.Contains(ni.Type!.ToString()))
                .Or(ni => ni.ContentType!.Equals("video/quicktime"));
    }
}
