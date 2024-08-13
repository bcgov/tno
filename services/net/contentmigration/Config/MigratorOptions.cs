namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// Stores options specific to an IContentMigrator implementation
/// </summary>
public class MigratorOptions
{
    /// <summary>
    /// List of supported Ingest Types
    /// </summary>
    public string[] SupportedIngests { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Source' field value
    /// doesn't match the Code or Name of any MMI Source
    /// The Key should be the "TNO 1.0 Source"
    /// The Value should be the "MMI Source Code"
    /// </summary>
    public Dictionary<string, string> IngestSourceMappings { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Stores a mapping to be used where the 'Source Code' has a Media Type that is not correctly mapped in MediaTypeMappings
    /// The Key should be the "MMI Source Code"
    /// The Value should be the "MMI Media Type Name"
    /// </summary>
    public Dictionary<string, string> SourceCodeMediaTypeMappings { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Type' field value
    /// doesn't match the Code or Name of any MMI Source
    /// The Key should be the "TNO 1.0 Type"
    /// The Value should be the "MMI Media Type Name"
    /// </summary>
    public Dictionary<string, string> MediaTypeMappings { get; set; } = new Dictionary<string, string>();
}
