using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationService class, provides a way to manage the syndication service.
/// </summary>
public class SyndicationService : ServiceManager<SyndicationDataSourceManager, SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationService object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationService(
        IApiService api,
        DataSourceManagerFactory<SyndicationDataSourceManager, SyndicationOptions> factory,
        IOptions<SyndicationOptions> options,
        ILogger<ServiceManager<SyndicationDataSourceManager, SyndicationOptions>> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion
}
