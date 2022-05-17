using TNO.API.Areas.Services.Models.DataSource;

namespace TNO.Services;

/// <summary>
/// IDataSourceIngestManager interface, provides a way to manage a single data source ingestion service.
/// </summary>
public interface IDataSourceIngestManager : IServiceActionManager
{
    #region Properties
    /// <summary>
    /// get - The data source managed by this object.
    /// </summary>
    public DataSourceModel DataSource { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public bool VerifyDataSource(DataSourceModel dataSource);
    #endregion
}
