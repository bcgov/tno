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

    /// <summary>
    /// get/set - For a cluster whose indexes exist but whose migrations index has no history (the
    /// indexes were created some other way): the version the cluster is treated as already at.
    /// Recorded before migrating, so earlier migrations are not run against existing indexes.
    /// </summary>
    public string BaselineVersion { get; set; } = "";
    /// <summary>Native reindex throttle, documents per second; -1 disables throttling.</summary>
    public long ReindexRequestsPerSecond { get; set; } = -1;

    /// <summary>
    /// get/set - The step of a migration to run ('all' runs every step in order). Only migrations
    /// that support steps accept anything else, one migration at a time, never for a rollback.
    /// </summary>
    public MigrationStepName Step { get; set; } = MigrationStepName.All;
    #endregion
}
