namespace TNO.Services;

/// <summary>
/// IServiceManager interface, provides the starting point for the service that runs beside the service api.
/// </summary>
public interface IServiceManager
{
    #region Properties
    /// <summary>
    /// get - The state of the service.
    /// </summary>
    public ServiceState State { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service manager.
    /// </summary>
    /// <returns></returns>
    public Task RunAsync();
    #endregion
}
