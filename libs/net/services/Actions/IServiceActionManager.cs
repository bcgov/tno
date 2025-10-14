namespace TNO.Services;

/// <summary>
/// IServiceActionManager interface, provides a way to manage a single action service.
/// </summary>
public interface IServiceActionManager
{
    #region Properties
    /// <summary>
    /// get - Whether the current manager is running.
    /// </summary>
    public bool IsRunning { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this action.
    /// </summary>
    /// <returns></returns>
    public Task RunAsync();

    /// <summary>
    /// Ask the process action(s) to stop.
    /// </summary>
    /// <returns></returns>
    public Task StopAsync();

    /// <summary>
    /// Inform action of successful run.
    /// </summary>
    /// <returns></returns>
    public Task RecordSuccessAsync();

    /// <summary>
    /// Inform action of failure.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public Task RecordFailureAsync(Exception? error = null);


    /// <summary>
    /// Send email alert of failure.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public Task SendEmailAsync(string subject, Exception ex);

    /// <summary>
    /// Send email alert of failure.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendEmailAsync(string subject, string message);
    #endregion
}
