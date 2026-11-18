
namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
///
/// </summary>
public class ContentMigratorFactory : IContentMigratorFactory
{
    private readonly IEnumerable<IContentMigrator> contentMigrators;

    /// <summary>
    ///
    /// </summary>
    /// <param name="contentMigrators"></param>
    public ContentMigratorFactory(IEnumerable<IContentMigrator> contentMigrators)
    {
        this.contentMigrators = contentMigrators;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ingestName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IContentMigrator GetContentMigrator(string? ingestName)
    {
        return contentMigrators
                .FirstOrDefault(x => x.SupportedIngests.Any(s => s == ingestName)) ?? throw new InvalidOperationException($"No supported strategy found for Ingest Name '{ingestName}'.");
    }
}

