using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.FileMonitor.Config;

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
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileMonitorManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IngestManagerFactory<FileMonitorIngestActionManager, FileMonitorOptions> factory,
        IOptions<FileMonitorOptions> options,
        ILogger<FileMonitorManager> logger)
        : base(serviceProvider, api, factory, options, logger)
    {
    }
    #endregion
}
