using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.CPNews.Config;

namespace TNO.Services.CPNews;

/// <summary>
/// CPNewsManager class, provides a way to manage the cpnews service.
/// </summary>
public class CPNewsManager : DataSourceManager<CPNewsDataSourceManager, CPNewsOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CPNewsManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CPNewsManager(
        IApiService api,
        DataSourceIngestManagerFactory<CPNewsDataSourceManager, CPNewsOptions> factory,
        IOptions<CPNewsOptions> options,
        ILogger<CPNewsManager> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion
}
