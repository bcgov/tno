using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Config;

namespace TNO.Services.Actions;

/// <summary>
/// ServiceAction abstract class, provides a generic service that runs a single action, via a service action manager.
/// When you inherit from this ServiceAction, you will need to provide the ServiceActionManager for your action.
/// </summary>
public abstract class ServiceAction<TOptions> : IServiceAction<TOptions>
    where TOptions : ServiceOptions
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The service to communicate with the api.
    /// </summary>
    public IApiService Api { get; private set; }

    /// <summary>
    /// get - The ingest configuration options.
    /// </summary>
    public TOptions Options { get; private set; }

    /// <summary>
    /// get/set - Logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ServiceAction(IApiService api, IOptions<TOptions> options, ILogger<ServiceAction<TOptions>> logger)
    {
        this.Api = api;
        this.Options = options.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified manager.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    public abstract Task PerformActionAsync<T>(IServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class;
    #endregion
}
