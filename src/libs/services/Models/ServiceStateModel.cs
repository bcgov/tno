namespace TNO.Services.Models;

/// <summary>
/// ServiceStateModel class, provides a model that returns the service state.
/// </summary>
public class ServiceStateModel
{
    #region Properties
    /// <summary>
    /// get/set - The current service state.
    /// </summary>
    public ServiceState? State { get; set; }
    #endregion
}
