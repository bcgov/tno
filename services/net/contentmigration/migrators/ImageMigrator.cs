using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Editor.Models.Topic;
using IngestModel = TNO.API.Areas.Services.Models.Ingest.IngestModel;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.Actions;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Sources.Oracle;
using System.Net;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
/// Implementation of IContentMigrator for Image
/// </summary>
public class ImageMigrator : IngestAction<ContentMigrationOptions>, IContentMigrator
{
    #region Variables

    /// <summary>
    /// stores options specific to this ContentMigrator
    /// </summary>
    readonly MigratorOptions migratorOptions;

    #endregion

    /// <summary>
    /// which Ingests this Migrator supports
    /// </summary>
    public IEnumerable<string> SupportedIngests { get {
        return this.migratorOptions.SupportedIngests;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptionsSnapshot<ContentMigrationOptions> options, ILogger<IngestAction<ContentMigrationOptions>> logger) : base(api, options, logger)
    {
        this.migratorOptions = migratorOptions.Get("ImageMigrator");
    }

    /// <summary>
    /// Creates an Image ContentReferenceModel from a NewsItem
    /// </summary>
    /// <param name="source"></param>
    /// <param name="topic"></param>
    /// <param name="newsItem"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(SourceModel source, string topic, NewsItem newsItem, string uid)
    {
        DateTime publishedOn = newsItem.ItemDateTime != null ?  newsItem.ItemDateTime.Value : DateTime.MinValue;
        DateTime publishedOnAsUTC = new DateTime(publishedOn.Ticks, DateTimeKind.Utc);

        return new ContentReferenceModel()
        {
            Source = source.Code,
            Uid = uid,
            PublishedOn = publishedOnAsUTC, // this.ToTimeZone(publishedOn, ingest).ToUniversalTime(),
            Topic = topic,
            Status = (int)WorkflowStatus.InProgress
        };
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
    private SourceContent? CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem, string referenceUid)
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
    public System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter() {
        return PredicateBuilder.New<NewsItem>()
            .And(ni => ni.ContentType!.Equals("image/jpeg"));
    }

    /// <summary>
    /// Attempt to migrate a single item from the historic system via api.
    /// Checks if a content reference has already been created for each item before deciding whether to migrate it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="lookups"></param>
    /// <param name="ingests"></param>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    public async Task MigrateNewsItemAsync(
        IIngestServiceActionManager manager,
        LookupModel? lookups,
        IEnumerable<IngestModel> ingests,
        NewsItem newsItem)
    {
        if (lookups == null) {
            this.Logger.LogError("Lookups cannot be null");
            throw new ArgumentNullException(nameof(lookups));
        }

        try
        {
            SourceModel? source = GetSourceMapping(lookups.Sources, newsItem.Source);
            if (source == null) {
                // if we don't have a custom mapping, fallback to our Ingest 'DEFAULT' mapping
                // TODO: KGM - When this happens, store the TNO 1.0 "Source" in "Other Source" field
                source = GetSourceMapping(lookups.Sources, manager.Ingest.Source!.Code);
                if (source == null) {
                    this.Logger.LogWarning("Couldn't map to Source for NewsItem with source '{sourceName}'", newsItem.Source);
                    return;
                }
            }
            ProductModel? product = GetProductMapping(lookups.Products, newsItem);
            if (product == null) {
                this.Logger.LogWarning("Couldn't map to Product for NewsItem with type '{sourceName}'", newsItem.Type);
                return;
            }

            // KGM: does uid exist for all newsItem types in old system and does this correlate to new system?
            // KGM: for soft-launch - how do we avoid duplication if MMIA is ingest from source *AND* historic?
            var uid = newsItem.WebPath ?? $"{newsItem.RSN}";
            var reference = await this.FindContentReferenceAsync(source?.Code, uid);
            if (reference == null)
            {
                reference = await this.Api.AddContentReferenceAsync(CreateContentReference(source!, manager.Ingest.Topic, newsItem, uid));
                Logger.LogInformation("Migrating content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
            }
            else if (reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow)
            {
                // If another process has it in progress only attempt to do an Migration if it's
                // more than an 5 minutes old. Assumption is that it is stuck.
                reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                Logger.LogInformation("Updating migrated content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
            }

            if (reference != null)
            {
                // TopicModel? topic = GetTopicMapping(lookups.Topics, newsItem.EodCategory);
                await ContentReceivedAsync(manager, reference, CreateSourceContent(lookups,
                                                                                   source!,
                                                                                   product,
                                                                                   manager.Ingest.IngestType!.ContentType,
                                                                                   newsItem,
                                                                                   reference.Uid));
            }
        }
        catch (Exception ex)
        {
            // Reached limit return to ingest manager.
            if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                throw;

            this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
            await manager.RecordFailureAsync();
        }
    }

    private static TopicModel? GetTopicMapping(IEnumerable<TopicModel> topics, string newsItemTopic)
    {
        return topics.Where(s => s.Name == newsItemTopic).FirstOrDefault();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemSource"></param>
    /// <returns></returns>
    private SourceModel? GetSourceMapping(IEnumerable<SourceModel> lookup, string newsItemSource)
    {
        SourceModel? source = lookup.Where(s => (s.Name.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase)
                                                 || s.Code.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();

        // if the Name doesnt match one of our sources, use the extra mappings from the config
        if (source == null)
        {
            this.migratorOptions.IngestSourceMappings.TryGetValue(newsItemSource, out string? customMapping);
            source = lookup.Where(s => s.Code == customMapping).FirstOrDefault();
        }

        return source;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    private ProductModel? GetProductMapping(IEnumerable<ProductModel> lookup, NewsItem newsItem)
    {
        // TODO: KGM - what to do if we have no mapping - make nullable so we can skip it on migration
        ProductModel? product = lookup.Where(s => s.Name == newsItem.Type).FirstOrDefault();

        // if the Name doesnt match one of our products, use the extra mappings from the config
        if (product == null)
        {
            this.migratorOptions.ProductMappings.TryGetValue(newsItem.Type, out string? customMapping);
            product = lookup.Where(s => s.Name == customMapping).FirstOrDefault();
        }

        return product;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException();
    }
}
