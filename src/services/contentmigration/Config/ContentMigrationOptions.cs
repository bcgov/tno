using TNO.Services.Config;

namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// ContentMigrationOptions class, configuration options for Import
/// </summary>
public class ContentMigrationOptions : IngestServiceOptions
{
    #region Properties

    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - The folder inside the mapped volume where migrated files will be staged.
    /// </summary>
    public string ContentStagingFolder { get; set; } = "_tmp";

    /// <summary>
    /// get/set - the web uri prefix to use for all files to import
    /// </summary>
    public string MediaHostRootUri { get; set; } = "";

    /// <summary>
    /// get/set - Settings to connect to the TNO 1.0 database.
    /// </summary>
    public OracleConnectionSettings? OracleConnection { get; set; } = null;

    /// <summary>
    /// get/set - The maximum number of records to retrieve from the TNO 1.0 db at a time
    /// </summary>
    public int MaxRecordsPerRetrieval { get; set; } = 20;

    /// <summary>
    /// get/set - The maximum number of records to migrate in a single ingest event
    /// </summary>
    public int MaxRecordsPerIngest { get; set; } = Int32.MaxValue;

    /// <summary>
    /// Mapping for "action type" to Name in the db
    /// Only need to add a mapping here, if the name in the db is different to the Action Type enum string
    /// </summary>
    public Dictionary<ActionType, string> ActionNameMappings { get; set; } = new Dictionary<ActionType, string>();

    /// <summary>
    /// get/set - The default number of minutes that it took a migrated article to be ready for publishing
    /// </summary>
    public float DefaultTimeTrackingValueInMinutes { get; set; } = 10F;

    /// <summary>
    /// get/set - The default activity for migrated articles
    /// </summary>
    public string DefaultTimeTrackingActivity { get; set; } = "Updated";

    /// <summary>
    /// get/set - The default username for any audit, ownership on migrated items
    /// </summary>
    public string DefaultUserNameForAudit { get; set; } = "admin";

    /// <summary>
    /// get/set - The default Tone value if none is set
    /// </summary>
    public int DefaultTone { get; set; } = 0;

    /// <summary>
    /// get/set - The Migration Type to do Historic|Recent
    /// </summary>
    public string SupportedImportMigrationTypes { get; set; } = "";

    /// <summary>
    /// get/set - Whether to only support ingest configurations for published content.
    /// </summary>
    public bool OnlyPublished { get; set; }

    /// <summary>
    /// get/set - Generate Alerts from content migrated from TNO 1.0 if it flagged as alertable.
    /// </summary>
    public bool GenerateAlertsOnContentMigration { get; set; } = true;

    /// <summary>
    /// get/set - The Migration Type to do Historic|Recent
    /// </summary>
    public string TagForMigratedContent { get; set; } = "";

    /// <summary>
    /// get/set - Whether to send a message to Kafka regardless of whether the content has been updated or not.
    /// </summary>
    public bool ForceUpdate { get; set; }
    #endregion
}
