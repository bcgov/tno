using System.Net;
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

        var newsItemTitle = newsItem.Title != null ? WebUtility.HtmlDecode(newsItem.Title) : string.Empty;

        // KGM: will the TNO filename match the MMI filename? Also, we don't want to risk having null or empty here...
        var referenceUid = newsItem.FileName ?? newsItem.RSN.ToString();

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            mediaType.Id,
            GetContentHash(source.Code, newsItem.GetTitle(), publishedOnInUtc),
            newsItemTitle,
            "",
            "",
            publishedOnInUtc,
            newsItem.Published)
        {
            FilePath = newsItem.FilePath ?? string.Empty,
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
        };

        if (newsItem.UpdatedOn != null)
        {
            var updatedOnInDefaultTimeZone = this.GetSourceDateTime(newsItem.UpdatedOn.Value, defaultTimeZone);
            var updatedOnInUtc = updatedOnInDefaultTimeZone.ToUniversalTime();
            Logger.LogDebug("NewItem.RSN: {rsn}, UpdatedDateTime: {publishedDateTime}, ToUtc: {publishedDateTimeInUtc}", newsItem.RSN, updatedOnInDefaultTimeZone, updatedOnInUtc);
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? updatedOnInUtc : null;
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

        return content;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter(ContentType contentType)
    {
        return PredicateBuilder.New<NewsItem>()
            .And(ni => ni.ContentType!.Equals("image/jpeg"));
    }
}
