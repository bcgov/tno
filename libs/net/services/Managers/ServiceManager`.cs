using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Config;

namespace TNO.Services.Managers;

/// <summary>
/// ServiceManager class, provides a way to manage the running of a service.
/// The primary role of the service manager is to manage the state of the current service.
/// </summary>
public abstract class ServiceManager<TOption> : IServiceManager
    where TOption : ServiceOptions
{
    #region Variables
    /// <summary>
    /// Api service controller.
    /// </summary>
    protected readonly IApiService _api;

    /// <summary>
    /// Configuration options for this service.
    /// </summary>
    protected readonly TOption _options;
    #endregion

    #region Properties
    /// <summary>
    /// get - The state of the service.
    /// </summary>
    public ServiceState State { get; private set; }

    /// <summary>
    /// get - Logger for this service.
    /// </summary>
    public ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api">Service to communicate with the api.</param>
    /// <param name="factory">Data source manager factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logging client.</param>
    public ServiceManager(
        IApiService api,
        IOptions<TOption> options,
        ILogger<ServiceManager<TOption>> logger)
    {
        _api = api;
        _options = options.Value;
        this.Logger = logger;
        this.State = new ServiceState(_options);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service manager.
    /// </summary>
    /// <returns></returns>
    public abstract Task RunAsync();
    #endregion
}
