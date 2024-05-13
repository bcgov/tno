using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
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
    /// get - Number of sequential failures.
    /// </summary>
    public int FailureCount { get; private set; }

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

    /// <summary>
    /// get - CHES service.
    /// </summary>
    protected IChesService Ches { get; }

    /// <summary>
    /// get - CHES options.
    /// </summary>
    protected ChesOptions ChesOptions { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api">Service to communicate with the api.</param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logging client.</param>
    public ServiceManager(
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<TOption> options,
        ILogger<ServiceManager<TOption>> logger)
    {
        this.Api = api;
        // All requests will be identified by the service type name.
        this.Api.OpenClient.Client.DefaultRequestHeaders.Add("User-Agent", GetType().FullName);
        this.Ches = chesService;
        this.ChesOptions = chesOptions.Value;
        this.Options = options.Value;
        this.Logger = logger;
        this.State = new ServiceState(this.Options);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send email alert of failure.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendEmailAsync(string subject, string message)
    {
        if (this.Options.SendEmailOnFailure)
        {
            try
            {
                var emailToList = this.Options.EmailTo?.Split(',').Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
                if (emailToList.Any())
                {
                    var email = new TNO.Ches.Models.EmailModel(this.ChesOptions.From, emailToList, subject, message);
                    await this.Ches.SendEmailAsync(email);
                }
                else
                {
                    this.Logger.LogDebug("No email addresses configured to receive errors");
                }
            }
            catch (ChesException ex)
            {
                this.Logger.LogError(ex, "Ches exception while sending email. {response}", ex.Data.Contains("body") ? ex.Data["body"] : ex.Message);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Email failed to send. {error}", ex.Data);
            }
        }
    }

    /// <summary>
    /// Send email alert of failure.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public async Task SendEmailAsync(string subject, Exception ex)
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        string? serviceName = GetType().FullName ?? "Service";
        string errorMsg = $"<div>An error occurred while executing the {serviceName} service.</div>{Environment.NewLine}" +
        $"<div>Environment: {env}</div>{Environment.NewLine}" +
        $"<div>Error Message:</div>{Environment.NewLine}" +
        $"<div>{ex.Message}</div>{Environment.NewLine}" +
        $"<div>StackTrace:</div>{Environment.NewLine}" +
        $"<div>{ex.StackTrace}</div>";
        await this.SendEmailAsync($"{env} - {serviceName} Service - {subject}", errorMsg);
    }

    /// <summary>
    /// Run the service manager.
    /// </summary>
    /// <returns></returns>
    public abstract Task RunAsync();
    #endregion
}
