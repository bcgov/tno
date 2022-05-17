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
    public Task PerformActionAsync(IServiceActionManager manager);
    #endregion
}
