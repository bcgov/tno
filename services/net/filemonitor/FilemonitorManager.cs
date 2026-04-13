using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail;
using TNO.Services.FileMonitor.Config;
using TNO.Services.Managers;

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
    /// <param name="emailService"></param>
    /// <param name="smtpOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileMonitorManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        IngestManagerFactory<FileMonitorIngestActionManager, FileMonitorOptions> factory,
        IOptions<FileMonitorOptions> options,
        ILogger<FileMonitorManager> logger)
        : base(serviceProvider, api, emailService, smtpOptions, factory, options, logger)
    {
    }
    #endregion
}
