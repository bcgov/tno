using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// DataSourceManagerFactory class, provides a way to create DataSourceManager objects.
/// </summary>
public class DataSourceManagerFactory<TDataSourceManager, TOption>
    where TDataSourceManager : IDataSourceManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IApiService _api;
    private readonly IIngestAction<TOption> _action;
    private readonly ILogger<TDataSourceManager> _logger;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceManagerFactory object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="action"></param>
    /// <param name="logger"></param>
    public DataSourceManagerFactory(IApiService api, IIngestAction<TOption> action, ILogger<TDataSourceManager> logger)
    {
        _api = api;
        _action = action;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a new instance of a DataSourceManager object.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public TDataSourceManager Create(DataSourceModel dataSource)
    {
        // var type = typeof(TDataSourceManager).MakeGenericType(new[] { typeof(DataSourceModel), typeof(IIngestAction<TOption>), typeof(IApiService), typeof(ILogger<TDataSourceManager>) });
        return (TDataSourceManager)(Activator.CreateInstance(typeof(TDataSourceManager), new object[] { dataSource, _action, _api, _logger }) ?? throw new InvalidOperationException("Unable to create instance of type"));
    }
    #endregion
}
