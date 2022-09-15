using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// IIngestAction interface, provides a way to configure what action this service performs when a ingest is run.
/// </summary>
public interface IIngestAction<TOptions> : IServiceAction<TOptions>
    where TOptions : IngestServiceOptions
{
    #region Properties
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified ingest.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    public Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class;
    #endregion
}
