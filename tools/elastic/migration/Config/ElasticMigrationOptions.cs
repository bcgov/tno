using TNO.DAL.Config;

namespace TNO.Elastic.Migration;

/// <summary>
/// ElasticMigrationOptions class, provides a way to configure the Elasticsearch migration tool
/// </summary>
public class ElasticMigrationOptions : ElasticOptions
{
    #region Properties
    /// <summary>
    /// get/set - The index name to hold applied migrations.
    /// </summary>
    public string MigrationIndex { get; set; } = "migrations";

    /// <summary>
    /// get/set - The number of replicas.
    /// </summary>
    public int? NumberOfReplicas { get; set; }

    /// <summary>
    /// get/set - The number of shards.
    /// </summary>
    public int? NumberOfShards { get; set; }

    /// <summary>
    /// get/set - The migration version to apply to Elasticsearch.
    /// </summary>
    public string MigrationVersion { get; set; } = "";

    /// <summary>
    /// get/set - The path to the folder containing the migration files.
    /// </summary>
    public string MigrationsPath { get; set; } = $".{Path.DirectorySeparatorChar}Migrations";

    /// <summary>
    /// get/set - The number of milliseconds to wait after each reindex loop.
    /// </summary>
    public int ReindexDelay { get; set; } = 5000;

    /// <summary>
    /// get/set - Maximum number of failures before exiting the reindex process.
    /// </summary>
    public int ReindexFailureLimit { get; set; } = 5;
    #endregion
}
