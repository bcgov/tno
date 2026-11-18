using System.Linq.Expressions;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.MediaType;
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
    Expression<Func<T, bool>> GetBaseFilter<T>(ContentType contentType)
        where T : BaseNewsItem => throw new NotImplementedException();

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
    /// <param name="source"></param>
    /// <returns></returns>
    MediaTypeModel? GetMediaTypeMapping(IEnumerable<MediaTypeModel> lookup, string newsItemType, SourceModel source);

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
    /// <param name="mediaType"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, MediaTypeModel mediaType, ContentType contentType, NewsItem newsItem, string defaultTimeZone);

    /// <summary>
    /// Copies a file from the TNO store to a location where it can be picked up by MMI Content service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="contentStagingRoot"></param>
    /// <returns></returns>
    Task CopyFileAsync(FileMigrationModel request, string contentStagingRoot);
}
