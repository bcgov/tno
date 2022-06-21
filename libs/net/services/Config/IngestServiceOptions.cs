namespace TNO.Services.Config;

/// <summary>
/// IngestServiceOptions class, configuration options for ingestion services.
/// </summary>
public class IngestServiceOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of media type this service will ingest (comma separated).
    /// </summary>
    public string MediaTypes { get; set; } = "";
    #endregion

    #region Methods
    /// <summary>
    /// Get an array of media types.
    /// </summary>
    /// <returns></returns>
    public string[] GetMediaTypes()
    {
        return this.MediaTypes.Split(',').Select(v => v.Trim()).ToArray();
    }
    #endregion
}
