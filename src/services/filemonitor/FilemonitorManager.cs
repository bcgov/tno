using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.FileMonitor.Config;
using TNO.Ches;
using TNO.Ches.Configuration;

namespace TNO.Services.FileMonitor;

/// <summary>
/// FileMonitorManager class, provides a way to manage the syndication service.
/// </summary>
public class FileMonitorManager : IngestManager<FileMonitorIngestActionManager, FileMonitorOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileMonitorManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileMonitorManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IngestManagerFactory<FileMonitorIngestActionManager, FileMonitorOptions> factory,
        IOptions<FileMonitorOptions> options,
        ILogger<FileMonitorManager> logger)
        : base(serviceProvider, api, chesService, chesOptions, factory, options, logger)
    {
    }
    #endregion
}
