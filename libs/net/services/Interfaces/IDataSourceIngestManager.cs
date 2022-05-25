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

    /// <summary>
    /// get - A dictionary of values that can be stored with this manager.
    /// </summary>
    public Dictionary<string, object> Values { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public bool VerifyDataSource();
    #endregion
}
