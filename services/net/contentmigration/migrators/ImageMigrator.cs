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
/// Implementation of IContentMigrator for Image
/// </summary>
public class ImageMigrator : ContentMigrator<ContentMigrationOptions>, IContentMigrator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrator<ContentMigrationOptions>> logger) : base(api, migratorOptions, options, logger)
    {
    }

    /// <summary>
    /// Creates an Image SourceContent from a NewsItem
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
            "",
            "",
            publishedOn.ToUniversalTime(),
            newsItem.Published)
        {
            // StreamUrl = ingest.GetConfigurationValue("url"),
            // FilePath = (ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "")
            //     .CombineWith($"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/", reference.Uid),
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
        };

        if (newsItem.UpdatedOn != null) {
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? newsItem.UpdatedOn.Value.ToUniversalTime() : null;
        }

        return content;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter() {
        return PredicateBuilder.New<NewsItem>()
            .And(ni => ni.ContentType!.Equals("image/jpeg"));
    }
}
