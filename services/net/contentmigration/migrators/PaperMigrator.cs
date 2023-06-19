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
            newsItem.Text ?? newsItemTitle, // KGM - This *should* work in TEST/PROD - CLOB tables are not part of a db export
            publishedOn.ToUniversalTime(),
            newsItem.Published)
        {
            Section = newsItem.string2 ?? string.Empty,
            Page = newsItem.string3 ?? string.Empty,
            FilePath = newsItem.FilePath ?? string.Empty,
            Link = newsItem.WebPath ?? string.Empty,
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            //Tags = item.Categories.Select(c => new TNO.Kafka.Models.Tag(c.Name, c.Label))

        };

        if ((newsItem.Tones != null) && (newsItem.Tones.Any())) {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMIA user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool { Value = (int)t.ToneValue, UserIdentifier = t.UserRSN.ToString() });
        }

        // Extract authors from a "delimited" string.  Don't use the source name as an author.
        if (!string.IsNullOrEmpty(newsItem.string5)) {
             content.Authors = ExtractAuthors(newsItem.string5, newsItem.Source).Select(a => new Author(a));
        }
        if (newsItem.UpdatedOn != null) {
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? newsItem.UpdatedOn.Value.ToUniversalTime() : null;
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory)) {
            content.Topics = new[] { new Kafka.Models.Topic {Name = newsItem.EodCategory, TopicType = (TopicType)Enum.Parse(typeof(TopicType), newsItem.EodGroup)}};
        }

        return content;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter() {
        return PredicateBuilder.New<NewsItem>()
                               .And(ni => !ni.ContentType!.Equals("video/quicktime"))
                               .And(ni => !ni.ContentType!.Equals("image/jpeg"));
    }
}
