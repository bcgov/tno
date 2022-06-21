using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// IServiceAction interface, provides a way to configure what action this service performs when it is run.
/// </summary>
public interface IServiceAction<TOptions>
    where TOptions : ServiceOptions
{
    #region Properties
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified manager.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    public Task PerformActionAsync<T>(IServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class;
    #endregion
}
