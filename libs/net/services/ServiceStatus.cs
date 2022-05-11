namespace TNO.Services;

/// <summary>
/// ServiceStatus enum, provides status options.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// The service is stopped.
    /// </summary>
    Stopped = 0,
    /// <summary>
    /// The service is running.
    /// </summary>
    Running = 1,
    /// <summary>
    /// The service is paused.
    /// A pause will only occur if a user has requested it.
    /// </summary>
    Paused = 2,
    /// <summary>
    /// The service is sleeping.
    /// Sleeping can occur if failures have reached their maximum.
    /// </summary>
    Sleeping = 3,
}
