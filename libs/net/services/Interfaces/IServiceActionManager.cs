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

    /// <summary>
    /// get - The number of times this action process has been run.
    /// </summary>
    public int RanCounter { get; }
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
    /// <returns></returns>
    public Task RecordFailureAsync();
    #endregion
}
