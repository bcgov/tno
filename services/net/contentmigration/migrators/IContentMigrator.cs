using System.Linq.Expressions;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Models;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
/// Interface for ContentMigrator implementations
/// </summary>
public interface IContentMigrator
{
    /// <summary>
    /// which Ingests this Migrator supports
    /// </summary>
    IEnumerable<string> SupportedIngests { get; }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Expression<Func<NewsItem, bool>> GetBaseFilter(ContentType contentType);

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemSource"></param>
    /// <returns></returns>
    SourceModel? GetSourceMapping(IEnumerable<SourceModel> lookup, string? newsItemSource);

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemType"></param>
    /// <returns></returns>
    ProductModel? GetProductMapping(IEnumerable<ProductModel> lookup, string newsItemType);

    /// <summary>
    /// Creates an ContentReferenceModel from a NewsItem
    /// </summary>
    /// <param name="source"></param>
    /// <param name="topic"></param>
    /// <param name="newsItem"></param>
    /// <param name="uid"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    ContentReferenceModel CreateContentReference(SourceModel source, string topic, NewsItem newsItem, string uid, string defaultTimeZone);

    /// <summary>
    /// Creates a SourceContent item
    /// </summary>
    /// <param name="lookups"></param>
    /// <param name="source"></param>
    /// <param name="product"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem, string defaultTimeZone);

    /// <summary>
    /// Copies a file from the TNO store to a location where it can be picked up by MMIA Content service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="contentStagingRoot"></param>
    /// <returns></returns>
    Task CopyFileAsync(FileMigrationModel request, string contentStagingRoot);
}
