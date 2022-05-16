using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationManager class, provides a way to manage the syndication service.
/// </summary>
public class SyndicationManager : DataSourceManager<SyndicationDataSourceManager, SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationManager(
        IApiService api,
        DataSourceIngestManagerFactory<SyndicationDataSourceManager, SyndicationOptions> factory,
        IOptions<SyndicationOptions> options,
        ILogger<DataSourceManager<SyndicationDataSourceManager, SyndicationOptions>> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion
}
