using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
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

    /// <summary>
    /// get - Configuration options for this service.
    /// </summary>
    protected TOption Options { get; private set; }

    /// <summary>
    /// get - Api service controller.
    /// </summary>
    protected IApiService Api { get; private set; }

    protected HttpRequestHeaders Headers
    {
        get
        {
            var headers = new HttpRequestMessage().Headers;
            headers.Add("User-Agent", GetType().Name);
            return headers;
        }
    }
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
        this.Api = api;
        this.Options = options.Value;
        this.Logger = logger;
        this.State = new ServiceState(this.Options);
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
