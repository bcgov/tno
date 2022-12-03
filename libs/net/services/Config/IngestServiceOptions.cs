namespace TNO.Services.Config;

/// <summary>
/// IngestServiceOptions class, configuration options for ingestion services.
/// </summary>
public class IngestServiceOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The type of service that will be run (i.e. stream|clip|tuner|RPi).
    /// </summary>
    public string ServiceType { get; set; } = "";

    /// <summary>
    /// get/set - An array of ingest type this service will ingest (comma separated) (i.e. "Audio, Video").
    /// </summary>
    public string IngestTypes { get; set; } = "";

    /// <summary>
    /// get/set - The data location this service is being run in.
    /// This provides context information for content that is stored on local volumes.
    /// </summary>
    public string DataLocation { get; set; } = "";

    /// <summary>
    /// get/set - If requests to the API fails to return new ingest, continue and reuse currently configured ingests.
    /// This is helpful to keep the service running even when the API fails.
    /// The goal is that the service will continue to perform part of its work even if it cannot communicate with the API.
    /// </summary>
    public bool ReuseIngests { get; set; } = false;
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
