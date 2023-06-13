using TNO.Services.Config;

namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// ContentMigrationOptions class, configuration options for Import
/// </summary>
public class ImageMigrationOptions : IngestServiceOptions
{
    #region Properties

    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Source' field value
    /// doesn't match the Code or Name of any MMIA Source
    /// The Key should be the "TNO 1.0 Source"
    /// The Value should be the "MMIA Source Code"
    /// </summary>
    public Dictionary<string,string> IngestSourceMappings {get; set; } = new Dictionary<string,string>();

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Type' field value
    /// doesn't match the Code or Name of any MMIA Source
    /// The Key should be the "TNO 1.0 Type"
    /// The Value should be the "MMIA Product Name"
    /// </summary>
    public Dictionary<string, string> ProductMappings { get; set; } = new Dictionary<string, string>()
        {
            { "Newspaper", "Corporate Calendar" }
        };

    #endregion
}
