using TNO.API.Areas.Services.Models.DataSource;

namespace TNO.Services;

/// <summary>
/// IDataSourceManager interface, provides a way to manage a single data source ingestion service.
/// </summary>
public interface IDataSourceManager
{
    #region Properties
    /// <summary>
    /// get - Whether the current manager is running.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// get - The number of times this data source process has been run.
    /// </summary>
    public int RanCounter { get; }

    /// <summary>
    /// get - The data source managed by this object.
    /// </summary>
    public DataSourceModel DataSource { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    /// <returns></returns>
    public Task RunAsync();

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public bool VerifyDataSource(DataSourceModel dataSource);

    /// <summary>
    /// Inform data source of successful run.
    /// </summary>
    /// <returns></returns>
    public Task<DataSourceModel> RecordSuccessfulRunAsync();

    /// <summary>
    /// Inform data source of failure.
    /// </summary>
    /// <returns></returns>
    public Task<DataSourceModel> RecordFailureAsync();
    #endregion
}
