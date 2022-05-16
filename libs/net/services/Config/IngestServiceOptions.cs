namespace TNO.Services.Config;

/// <summary>
/// IngestServiceOptions class, configuration options for ingestion services.
/// </summary>
public class IngestServiceOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of media type this service will ingest.
    /// </summary>
    public string[] MediaTypes { get; set; } = Array.Empty<string>();
    #endregion
}
