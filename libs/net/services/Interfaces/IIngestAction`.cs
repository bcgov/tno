using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// IIngestAction interface, provides a way to configure what action this service performs when a data source is run.
/// </summary>
public interface IIngestAction<TOptions> : IServiceAction<TOptions>
    where TOptions : IngestServiceOptions
{
    #region Properties
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified data source.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    public Task PerformActionAsync(IDataSourceIngestManager dataSource, string? name = null, CancellationToken cancellationToken = default);
    #endregion
}
