using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandDataSourceManager class, provides a way to manage the command ingestion process for this data source.
/// </summary>
public class CommandDataSourceManager : DataSourceIngestManager<CommandOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CommandDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<CommandOptions> action, IOptions<CommandOptions> options)
        : base(dataSource, api, action, options)
    {
    }
    #endregion

    #region Methods
    #endregion
}
