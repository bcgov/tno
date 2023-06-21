using System.Net;
using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Config;
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
    /// <param name="referenceUid"></param>
    /// <returns></returns>
    public override SourceContent? CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem, string referenceUid)
    {
        // var authors = GetAuthors(lookups.Contributors)
        var publishedOn = newsItem.ItemDateTime ?? DateTime.UtcNow;

        var newsItemTitle = newsItem.Title != null ? WebUtility.HtmlDecode(newsItem.Title) : string.Empty;

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            product.Id,
            referenceUid,
            newsItemTitle,
            newsItem.Summary! ?? string.Empty,
            newsItem.Transcript ?? string.Empty,
            publishedOn.ToUniversalTime(),
            newsItem.Published)
        {
            // StreamUrl = ingest.GetConfigurationValue("url"),
            // FilePath = (ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "")
            //     .CombineWith($"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/", reference.Uid),
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            //Tags = item.Categories.Select(c => new TNO.Kafka.Models.Tag(c.Name, c.Label))

        };

        // newsItem.string5 & newsItem.string5 seem to be the "Show/Program"

        if (newsItem.UpdatedOn != null)
        {
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? newsItem.UpdatedOn.Value.ToUniversalTime() : null;
        }

        if (newsItem.Tones != null)
        {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMIA user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool { Value = (int)t.ToneValue, UserIdentifier = t.UserRSN.ToString() });
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory))
        {
            content.Topics = new[] { new Kafka.Models.Topic { Name = newsItem.EodCategory, TopicType = (TopicType)Enum.Parse(typeof(TopicType), newsItem.EodGroup) } };
        }

        // Tags are in the Summary as they are added by an Editor
        if (!string.IsNullOrEmpty(newsItem.Summary)) {
            // if Tags are found, let the ContentManagement service decide if they are new or not
            content.Tags = this.ExtractTags(newsItem.Summary)
                .Select(c => new TNO.Kafka.Models.Tag(c.ToUpperInvariant(),""));
        }

        // map relevant news item properties to actions
        content.Actions = GetActionMappings(newsItem.FrontPageStory, newsItem.WapTopStory, newsItem.Alert,
            newsItem.Commentary, newsItem.CommentaryTimeout);

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
