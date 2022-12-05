using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Capture.Config;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureManager class, provides a way to manage the capture service.
/// </summary>
public class CaptureManager : IngestManager<CaptureIngestActionManager, CaptureOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureManager(
        IApiService api,
        IngestManagerFactory<CaptureIngestActionManager, CaptureOptions> factory,
        IOptions<CaptureOptions> options,
        ILogger<CaptureManager> logger)
        : base(api, factory, options, logger)
    {
    }

    /// <summary>
    /// Get the data sources for the capture services.
    /// Only data sources of serviceType=stream.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var ingests = await base.GetIngestsAsync();
        var serviceType = !String.IsNullOrWhiteSpace(this.Options.ServiceType) ? this.Options.ServiceType : "stream";
        var hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");

        return ingests.Where(i =>
            i.GetConfigurationValue("serviceType") == serviceType &&
            (String.IsNullOrWhiteSpace(i.GetConfigurationValue("hostname")) ||
                i.GetConfigurationValue("hostname") == hostname) &&
            (String.IsNullOrWhiteSpace(this.Options.DataLocation) ||
                i.DataLocations.Any(dl => dl.Name == this.Options.DataLocation))
            );
    }
    #endregion
}
