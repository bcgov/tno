using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Capture.Config;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Models.Extensions;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureManager class, provides a way to manage the capture service.
/// </summary>
public class CaptureManager : DataSourceManager<CaptureDataSourceManager, CaptureOptions>
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
        DataSourceIngestManagerFactory<CaptureDataSourceManager, CaptureOptions> factory,
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
    public override async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync()
    {
        var dataSources = await base.GetDataSourcesAsync();

        return dataSources.Where(ds => IsStream(ds));
    }

    /// <summary>
    /// Determine if the data source is a stream service type.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private static bool IsStream(DataSourceModel dataSource)
    {
        return dataSource.GetConnectionValue("serviceType") == "stream";
    }
    #endregion
}
