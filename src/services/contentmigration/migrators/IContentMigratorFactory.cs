namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
///
/// </summary>
public interface IContentMigratorFactory
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="ingestName"></param>
    /// <returns></returns>
    IContentMigrator GetContentMigrator(string ingestName);
}
