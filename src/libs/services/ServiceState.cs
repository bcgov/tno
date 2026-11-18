
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// ServiceState record, provides a way to watch the current service state.
/// </summary>
public class ServiceState
{
    #region Properties
    /// <summary>
    /// get - The status of the service.
    /// </summary>
    public ServiceStatus Status { get; private set; } = ServiceStatus.Running;

    /// <summary>
    /// get - The number of sequential failures that have occurred.
    /// </summary>
    public int Failures { get; private set; }

    /// <summary>
    /// get - Maximum number of retries after a failure.
    /// </summary>
    public int MaxFailureLimit { get; private set; } = 3;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceState object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public ServiceState(ServiceOptions options)
    {
        this.MaxFailureLimit = options.MaxFailureLimit;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Record a failure.
    /// </summary>
    /// <returns></returns>
    public int RecordFailure()
    {
        this.Failures++;

        if (this.Failures >= this.MaxFailureLimit && this.Status != ServiceStatus.Failed) this.Status = ServiceStatus.RequestFailed;
        return this.Failures;
    }

    /// <summary>
    /// Reset the failures back to zero.
    /// </summary>
    public void ResetFailures()
    {
        this.Failures = 0;
    }

    /// <summary>
    /// Change the status to sleeping.
    /// </summary>
    public void Sleep()
    {
        this.Status = ServiceStatus.RequestSleep;
    }

    /// <summary>
    /// Change the status to paused.
    /// </summary>
    public void Pause()
    {
        this.Status = ServiceStatus.RequestPause;
    }

    /// <summary>
    /// Change the status to either Paused or Sleeping depending on current state.
    /// </summary>
    public void Stop()
    {
        if (this.Status == ServiceStatus.RequestSleep)
            this.Status = ServiceStatus.Sleeping;
        else if (this.Status == ServiceStatus.RequestPause)
            this.Status = ServiceStatus.Paused;
        else if (this.Status == ServiceStatus.RequestFailed)
            this.Status = ServiceStatus.Failed;
        else Sleep();
    }

    /// <summary>
    /// Change the status to running.
    /// </summary>
    public void Resume()
    {
        ResetFailures();
        this.Status = ServiceStatus.Running;
    }
    #endregion
}
