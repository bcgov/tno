
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
    public IContentMigrator GetContentMigrator(string ingestName)
    {
        var supportedStrategy = contentMigrators
                .FirstOrDefault(x => x.SupportedIngests.Any(s => s == ingestName));

        if (supportedStrategy == null)
            throw new InvalidOperationException($"No supported strategy found for Ingest Name '{ingestName}'.");

        return supportedStrategy;
    }
}

