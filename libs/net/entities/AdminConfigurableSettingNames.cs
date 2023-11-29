using TNO.Core.Json;

namespace TNO.Entities;

/// <summary>
/// Provides report status.
/// </summary>
public enum AdminConfigurableSettingNames
{
    /// <summary>
    /// Report is pending to be sent.
    /// </summary>
    [EnumValue("ProductSubscriptionManagerEmail")]
    ProductSubscriptionManagerEmail,
}
