using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Capture.Config;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;
using TNO.Ches;
using TNO.Ches.Configuration;

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
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IngestManagerFactory<CaptureIngestActionManager, CaptureOptions> factory,
        IOptions<CaptureOptions> options,
        ILogger<CaptureManager> logger)
        : base(serviceProvider, api, chesService, chesOptions, factory, options, logger)
    {
    }

    /// <summary>
    /// Get the ingest configurations for the capture services.
    /// Filter out ingests based on configuration.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var ingests = await base.GetIngestsAsync();
        var hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");
        var serviceTypes = this.Options.GetServiceTypes();

        return ingests.Where(i =>
            (!serviceTypes.Any() || serviceTypes.Any(st => st == i.GetConfigurationValue("serviceType"))) &&
            (String.IsNullOrWhiteSpace(i.GetConfigurationValue("hostname")) ||
                i.GetConfigurationValue("hostname") == hostname) &&
            (String.IsNullOrWhiteSpace(this.Options.DataLocation) ||
                i.DataLocations.Any(dl => dl.Name == this.Options.DataLocation))
            );
    }
    #endregion
}
