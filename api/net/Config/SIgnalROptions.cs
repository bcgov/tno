namespace TNO.API.Config;

/// <summary>
/// SignalROptions class, provides a way to configure SignalR.
/// </summary>
public class SignalROptions
{
    #region Properties
    /// <summary>
    /// get/set - Whether to enable the Kafka back plane.
    /// </summary>
    public bool EnableKafkaBackPlane { get; set; } = true;

    /// <summary>
    /// get/set - Path pattern to SignalR hub.
    /// </summary>
    public string HubPath { get; set; } = "/hub";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool EnableDetailedErrors { get; set; }
    #endregion
}
