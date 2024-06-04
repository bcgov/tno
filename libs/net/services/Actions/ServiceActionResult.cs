namespace TNO.Services.Actions;

/// <summary>
/// ServiceActionResult enum, encapsulates how a service action completed.
/// </summary>
public enum ServiceActionResult
{
    /// <summary>
    /// Action failed.
    /// </summary>
    Failure = 0,
    /// <summary>
    /// Action completed successfully.
    /// </summary>
    Success = 1,
    /// <summary>
    /// Skip the action.
    /// </summary>
    Skipped = 2
}
