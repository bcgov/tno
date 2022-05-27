using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Clip.Config;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Models.Extensions;

namespace TNO.Services.Clip;

/// <summary>
/// ClipManager class, provides a way to manage the clip service.
/// </summary>
public class ClipManager : DataSourceManager<ClipDataSourceManager, ClipOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClipManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ClipManager(
        IApiService api,
        DataSourceIngestManagerFactory<ClipDataSourceManager, ClipOptions> factory,
        IOptions<ClipOptions> options,
        ILogger<ClipManager> logger)
        : base(api, factory, options, logger)
    {
    }

    /// <summary>
    /// Get the data sources for the clip services.
    /// Only data sources of serviceType=stream.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync()
    {
        var dataSources = await base.GetDataSourcesAsync();

        return dataSources.Where(ds => IsClip(ds));
    }

    /// <summary>
    /// Determine if the data source is a clip service type
    /// and it has a parent.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private static bool IsClip(DataSourceModel dataSource)
    {
        return dataSource.GetConnectionValue("serviceType") == "clip" &&
            dataSource.ParentId != null;
    }
    #endregion
}
