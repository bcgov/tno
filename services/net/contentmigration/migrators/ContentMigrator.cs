using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Services.Models.Ingest;
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
    System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter();

    /// <summary>
    ///
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="lookups"></param>
    /// <param name="ingests"></param>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    Task MigrateNewsItemAsync(IIngestServiceActionManager manager, LookupModel? lookups, IEnumerable<IngestModel> ingests, NewsItem newsItem);
}
