using Microsoft.Extensions.Options;
using TNO.Services.Config;

namespace TNO.Services.Actions;

/// <summary>
/// ServiceAction abstract class, provides a generic service that runs a ingestion action, via a data source action manager.
/// When you inherit from this IngestAction, you will need to provide the DataSourceIngestionManager for your action.
/// </summary>
public abstract class IngestAction<TOptions> : ServiceAction<TOptions>, IIngestAction<TOptions>
    where TOptions : IngestServiceOptions
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public IngestAction(IApiService api, IOptions<TOptions> options) : base(api, options)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified data source.
    /// </summary>
    /// <param name="manager"></param>
    public abstract Task PerformActionAsync(IDataSourceIngestManager manager);

    /// <summary>
    /// Perform the action for the specified data source.
    /// Override the PeformActionAsync(IDataSourceIngestManager manager) method instead of this one.
    /// </summary>
    /// <param name="manager"></param>
    public override Task PerformActionAsync(IServiceActionManager manager)
    {
        return PerformActionAsync((IDataSourceIngestManager)manager);
    }
    #endregion
}
