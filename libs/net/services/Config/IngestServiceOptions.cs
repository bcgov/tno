namespace TNO.Services.Config;

/// <summary>
/// IngestServiceOptions class, configuration options for ingestion services.
/// </summary>
public class IngestServiceOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of ingest type this service will ingest (comma separated).
    /// </summary>
    public string IngestTypes { get; set; } = "";
    #endregion

    #region Methods
    /// <summary>
    /// Get an array of ingest types.
    /// </summary>
    /// <returns></returns>
    public string[] GetIngestTypes()
    {
        return this.IngestTypes.Split(',').Select(v => v.Trim()).ToArray();
    }
    #endregion
}
