using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Filemonitor.Config;

namespace TNO.Services.Filemonitor;

/// <summary>
/// SyndicationManager class, provides a way to manage the syndication service.
/// </summary>
public class FilemonitorManager : DataSourceManager<FilemonitorDataSourceManager, FilemonitorOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FilemonitorManager(
        IApiService api,
        DataSourceIngestManagerFactory<FilemonitorDataSourceManager, FilemonitorOptions> factory,
        IOptions<FilemonitorOptions> options,
        ILogger<FilemonitorManager> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion
}
