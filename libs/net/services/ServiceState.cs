
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
    public int MaxRetryLimit { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceState object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public ServiceState(IngestServiceOptions options)
    {
        this.MaxRetryLimit = options.MaxRetryLimit;
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

        if (this.Failures >= this.MaxRetryLimit) this.Status = ServiceStatus.Sleeping;
        return this.Failures;
    }

    /// <summary>
    /// Reset the failures back to zero.
    /// Change status to resume if not running.
    /// </summary>
    public void ResetFailures()
    {
        if (this.Status != ServiceStatus.Running && this.Failures >= this.MaxRetryLimit)
            this.Status = ServiceStatus.Running;

        this.Failures = 0;
    }

    /// <summary>
    /// Change the status to sleeping.
    /// </summary>
    public void Sleep()
    {
        this.Status = ServiceStatus.Sleeping;
    }

    /// <summary>
    /// Change the status to paused.
    /// </summary>
    public void Pause()
    {
        this.Status = ServiceStatus.Paused;
    }

    /// <summary>
    /// Change the status to paused.
    /// </summary>
    public void Resume()
    {
        this.Status = ServiceStatus.Running;
    }
    #endregion
}
