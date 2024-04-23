using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Extensions;
using TNO.Services.Config;

namespace TNO.Services.Actions.Managers;

/// <summary>
/// ServiceActionManager class, provides a way to manage the ingestion process for this data source.
/// </summary>
public abstract class ServiceActionManager<TOptions> : IServiceActionManager
    where TOptions : ServiceOptions
{
    #region Variables
    private readonly IServiceAction<TOptions> _action;
    private readonly IChesService _ches;
    private readonly ChesOptions _chesOptions;
    #endregion

    #region Properties
    /// <summary>
    /// get - Whether the current manager is running.
    /// </summary>
    public bool IsRunning { get; protected set; }

    /// <summary>
    /// get - The number of times this data source process has been run.
    /// </summary>
    public int RanCounter { get; protected set; }

    /// <summary>
    /// get - Configuration options for the service.
    /// </summary>
    protected TOptions Options { get; private set; }

    /// <summary>
    /// get - The action manager logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceIngestManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ServiceActionManager(
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IServiceAction<TOptions> action,
        IOptions<TOptions> options,
        ILogger<IServiceActionManager> logger)
    {
        _ches = ches;
        _chesOptions = chesOptions.Value;
        _action = action;
        this.Options = options.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    public virtual async Task RunAsync()
    {
        var run = await PreRunAsync();
        if (run && !this.IsRunning)
        {
            try
            {
                this.IsRunning = true;

                var actionResult = await PerformActionAsync();

                this.RanCounter++;

                await PostRunAsync(actionResult);
            }
            catch
            {
                throw;
            }
            finally
            {
                this.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// Stop the running action.
    /// </summary>
    /// <returns></returns>
    public virtual Task StopAsync()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Determine if the run should be executed.
    /// </summary>
    /// <returns></returns>
    protected virtual Task<bool> PreRunAsync()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Call the configured action.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual async Task<ServiceActionResult> PerformActionAsync(string? name = null)
    {
        // Perform configured action.
        return await PerformActionAsync<object>(name, null);
    }

    /// <summary>
    /// Call the configured action.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected virtual async Task<ServiceActionResult> PerformActionAsync<T>(string? name = null, T? data = null)
        where T : class
    {

        // Perform configured action.
        return await _action.PerformActionAsync(this, name, data);
    }

    /// <summary>
    /// Perform activity after a successful run.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task PostRunAsync(ServiceActionResult actionResult)
    {
        if (actionResult == ServiceActionResult.Success)
        {
            // Inform data source of run.
            await RecordSuccessAsync();
        }
    }

    /// <summary>
    /// Inform data source of successful run.
    /// </summary>
    /// <returns></returns>
    public abstract Task RecordSuccessAsync();

    /// <summary>
    /// Inform data source of failure.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public abstract Task RecordFailureAsync(Exception? error = null);

    /// <summary>
    /// Send email alert of failure.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public async Task SendEmailAsync(string subject, Exception ex)
    {
        await this.SendEmailAsync(subject, $"<div>{ex.GetAllMessages()}</div>{Environment.NewLine}<div>{ex.StackTrace}</div>");
    }

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
                var email = new TNO.Ches.Models.EmailModel(_chesOptions.From, emailToList, subject, message);
                await _ches.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Email failed to send");
            }
        }
    }
    #endregion
}
