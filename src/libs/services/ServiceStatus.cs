namespace TNO.Services;

/// <summary>
/// ServiceStatus enum, provides status options.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// The service is running.
    /// </summary>
    Running = 0,
    /// <summary>
    /// The service has been asked to pause, but it hasn't paused yet.
    /// </summary>
    RequestPause = 1,
    /// <summary>
    /// The service is paused.
    /// A pause will only occur if a user has requested it.
    /// </summary>
    Paused = 2,
    /// <summary>
    /// The service is has been asked to sleep, but it hasn't gone to sleep yet.
    /// </summary>
    RequestSleep = 3,
    /// <summary>
    /// The service is sleeping.
    /// Sleeping can occur if failures have reached their maximum.
    /// </summary>
    Sleeping = 4,
    /// <summary>
    /// The service has a failure and must stop.
    /// </summary>
    RequestFailed = 5,
    /// <summary>
    /// The service has failed.
    /// </summary>
    Failed = 6,
}
