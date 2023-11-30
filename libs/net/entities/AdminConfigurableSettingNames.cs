using TNO.Core.Json;

namespace TNO.Entities;

/// <summary>
/// Used to hold names of settings that can be configured in th Editor > Settings UI.
/// </summary>
public enum AdminConfigurableSettingNames
{
    /// <summary>
    /// Email for the admin responsible for processing report subscription requests.
    /// </summary>
    [EnumValue("ProductSubscriptionManagerEmail")]
    ProductSubscriptionManagerEmail,
}
