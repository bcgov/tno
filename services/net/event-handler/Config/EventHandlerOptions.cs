
using TNO.Services.Config;

namespace TNO.Services.EventHandler.Config;

/// <summary>
/// EventHandlerOptions class, configuration options for event handler service
/// </summary>
public class EventHandlerOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";
    #endregion
}
