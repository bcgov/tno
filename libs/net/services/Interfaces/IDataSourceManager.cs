using TNO.API.Areas.Services.Models.DataSource;

namespace TNO.Services;

/// <summary>
/// IDataSourceManager interface, provides a way to manage several data source schedules.
/// It will fetch all data sources for the configured media types.
/// It will ensure all data sources are being run based on their schedules.
/// </summary>
public interface IDataSourceManager : IServiceManager
{
    #region Propertiesx
    #endregion

    #region Methods
    public Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync();
    #endregion
}
