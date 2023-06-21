using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Topic;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
///
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public abstract class ContentMigrator<TOptions> : IContentMigrator
    where TOptions : ContentMigrationOptions
{
    #region Properties
    /// <summary>
    /// get - The service to communicate with the api.
    /// </summary>
    public IApiService Api { get; private set; }

    /// <summary>
    /// get - The ingest configuration options.
    /// </summary>
    public TOptions Options { get; private set; }

    /// <summary>
    /// get/set - Logger.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// stores options specific to this ContentMigrator
    /// </summary>
    protected MigratorOptions MigratorOptions { get; private set; }

    /// <summary>
    /// which Ingests this Migrator supports
    /// </summary>
    public IEnumerable<string> SupportedIngests { get {
        return this.MigratorOptions.SupportedIngests;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance of a ServiceAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<TOptions> options, ILogger<ContentMigrator<TOptions>> logger)
    {
        this.Api = api;
        this.Options = options.Value;
        this.Logger = logger;
        // as a convention the Named Option should be the same as the class name which needs to consume it
        this.MigratorOptions = migratorOptions.Get(this.GetType().Name);
    }
    #endregion

    #region Methods

    /// <summary>
    /// Gets the base filter for excluding items from the dbcontext search
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual Expression<Func<NewsItem, bool>> GetBaseFilter() => throw new NotImplementedException();

    /// <summary>
    /// Creates a SourceContent item
    /// </summary>
    /// <param name="lookups"></param>
    /// <param name="source"></param>
    /// <param name="product"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="referenceUid"></param>
    /// <returns></returns>
    public virtual SourceContent? CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem, string referenceUid) => throw new NotImplementedException();

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an Clip ContentReferenceModel from a NewsItem
    /// </summary>
    /// <param name="source"></param>
    /// <param name="topic"></param>
    /// <param name="newsItem"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public ContentReferenceModel CreateContentReference(SourceModel source, string topic, NewsItem newsItem, string uid)
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
    /// Get mapped topics from lookup
    /// </summary>
    /// <param name="topics"></param>
    /// <param name="newsItemTopic"></param>
    /// <returns></returns>
    public TopicModel? GetTopicMapping(IEnumerable<TopicModel> topics, string? newsItemTopic)
    {
        return topics.Where(s => s.Name == newsItemTopic).FirstOrDefault();
    }

    /// <summary>
    /// Extracts Authors from a source string, using any combination of delimters plus the word " and "
    /// </summary>
    /// <param name="authors"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static IEnumerable<string> ExtractAuthors(string authors, string source) {
        string[] delimiters = new [] { ",", ";", " ,", " & ", " and " };
        var splitArray = authors.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return splitArray.Where(s => !s.Equals(source));
    }

    /// <summary>
    /// extracts Tags from a source string
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal IEnumerable<string> ExtractTags(string source) {
        Regex tagsBetweenBracketsRegex = new(@"\[([^\]]*)\]", RegexOptions.RightToLeft);
        string[] tags = Array.Empty<string>();
        var tagMatches = tagsBetweenBracketsRegex.Matches(source);
        if (tagMatches.Count > 0) {
            var rawTags = tagMatches[0].Groups[1].Value.Split(',',StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // KGM : Fail on tags extract if any of the tags are not between 3 and 4 chars long
            // Do we need to make this validation better?
            if (rawTags.All(s => s.Length >= 3 && s.Length <= 4))
                tags = rawTags;
        }
        return tags;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemSource"></param>
    /// <returns></returns>
    public SourceModel? GetSourceMapping(IEnumerable<SourceModel> lookup, string newsItemSource)
    {
        SourceModel? source = lookup.Where(s => (s.Name.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase)
                                                 || s.Code.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();

        // if the Name doesnt match one of our sources, use the extra mappings from the config
        if (source == null)
        {
            this.MigratorOptions.IngestSourceMappings.TryGetValue(newsItemSource, out string? customMapping);
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
    public ProductModel? GetProductMapping(IEnumerable<ProductModel> lookup, NewsItem newsItem)
    {
        // TODO: KGM - what to do if we have no mapping - make nullable so we can skip it on migration
        ProductModel? product = lookup.Where(s => s.Name == newsItem.Type).FirstOrDefault();

        // if the Name doesnt match one of our products, use the extra mappings from the config
        if (product == null)
        {
            this.MigratorOptions.ProductMappings.TryGetValue(newsItem.Type, out string? customMapping);
            product = lookup.Where(s => s.Name == customMapping).FirstOrDefault();
        }

        return product;
    }

    #endregion
}
