namespace TNO.Services;

/// <summary>
/// Optional processing health used by services whose background work can stall independently of HTTP.
/// </summary>
public interface IServiceHealth
{
    /// <summary>
    /// get - The reason the worker requires a restart, or null when processing is healthy.
    /// </summary>
    string? HealthFailureReason { get; }
}
