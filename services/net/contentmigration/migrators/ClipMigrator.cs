using System.Configuration;
using System.Net;
using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Services.Models.ContentReference;
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
    /// <param name="product"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    public override SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem, string defaultTimeZone)
    {
        // var authors = GetAuthors(lookups.Contributors)

        // KGM: Would be nice to do this, but I don't think we could map a clip back to a schedule so there may be duplicates here
        // var referenceUid = $"{schedule.Name}:{schedule.Id}-{publishedOn:yyyy-MM-dd-hh-mm-ss}";
        var publishedOn = this.GetSourceDateTime(newsItem.GetPublishedDateTime(), defaultTimeZone).ToUniversalTime();

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            product.Id,
            GetContentHash(source.Code, newsItem.GetTitle(), publishedOn),
            newsItem.GetTitle(),
            newsItem.Summary! ?? string.Empty,
            newsItem.Transcript ?? string.Empty,
            publishedOn,
            newsItem.Published)
        {
            FilePath = newsItem.FilePath ?? string.Empty,
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
        };

        if (string.IsNullOrEmpty(this.Options.DefaultUserNameForAudit)) throw new System.Configuration.ConfigurationErrorsException("Default Username for ContentMigration has not been configured");
        var auditUser = lookups.Users.FirstOrDefault(u => u.Username == this.Options.DefaultUserNameForAudit);
        if (auditUser == null) throw new System.Configuration.ConfigurationErrorsException($"Default User for ContentMigration not found : {this.Options.DefaultUserNameForAudit}");

        // newsItem.string5 and newsItem.string5 both seem to be the "Show/Program"
        if (newsItem.string5 != null) {
            content.Series = newsItem.string5;
        }

        if (newsItem.UpdatedOn != null)
        {
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ?  this.GetSourceDateTime(newsItem.UpdatedOn.Value, defaultTimeZone).ToUniversalTime() : null;
        }

        if (newsItem.Tones?.Any() == true)
        {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMIA user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool { Value = (int)t.ToneValue, UserIdentifier = (t.UserRSN == 0 ? null : t.UserRSN.ToString()) });
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory))
        {
            content.Topics = new[] { new Kafka.Models.Topic { Name = newsItem.EodCategory, TopicType = (TopicType)Enum.Parse(typeof(TopicType), newsItem.EodGroup) } };
        }

        // Tags are in the Summary as they are added by an Editor
        if (!string.IsNullOrEmpty(newsItem.Summary)) {
            // if Tags are found, let the ContentManagement service decide if they are new or not
            content.Tags = ExtractTags(newsItem.Summary)
                .Select(c => new TNO.Kafka.Models.Tag(c.ToUpperInvariant(),""));
        }

        // map relevant news item properties to actions
        content.Actions = GetActionMappings(newsItem.FrontPageStory, newsItem.WapTopStory, newsItem.Alert,
            newsItem.Commentary, newsItem.CommentaryTimeout);

        // the total "Effort" is stored in the Number2 field as seconds
        if (newsItem.Number2.HasValue && newsItem.Number2 > 0) {
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
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter()
    {
        return PredicateBuilder.New<NewsItem>()
            .And(ni => ni.ContentType!.Equals("video/quicktime"));
    }
}
