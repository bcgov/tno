namespace TNO.Services;

/// <summary>
/// IServiceManager interface, provides a way to manage several data source schedules.
/// It will fetch all data sources for the configured media types.
/// It will ensure all data sources are being run based on their schedules.
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
