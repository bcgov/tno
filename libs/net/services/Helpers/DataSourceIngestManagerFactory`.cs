using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// DataSourceIngestManagerFactory class, provides a way to create DataSourceIngestManager objects.
/// </summary>
public class DataSourceIngestManagerFactory<TDataSourceIngestManager, TOption>
    where TDataSourceIngestManager : IDataSourceIngestManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IApiService _api;
    private readonly IIngestAction<TOption> _action;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceIngestManagerFactory object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="action"></param>
    public DataSourceIngestManagerFactory(IApiService api, IIngestAction<TOption> action)
    {
        _api = api;
        _action = action;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a new instance of a DataSourceIngestManager object.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public TDataSourceIngestManager Create(DataSourceModel dataSource)
    {
        // var type = typeof(TDataSourceIngestManager).MakeGenericType(new[] { typeof(DataSourceModel), typeof(IIngestAction<TOption>), typeof(IApiService), typeof(ILogger<TDataSourceIngestManager>) });
        return (TDataSourceIngestManager)(Activator.CreateInstance(typeof(TDataSourceIngestManager), new object[] { dataSource, _api, _action })
            ?? throw new InvalidOperationException($"Unable to create instance of type '{typeof(TDataSourceIngestManager).Name}'"));
    }
    #endregion
}
