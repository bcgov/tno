using System.Text.Json.Serialization;

namespace TNO.Keycloak.Models;

/// <summary>
/// ResourceAccessModel class, provides a model to deserialize JSON claim value.
/// </summary>
public class ResourceAccessModel
{
    #region Properties
    /// <summary>
    /// get/set - Realm management claims.
    /// </summary>
    [JsonPropertyName("realm-management")]
    public RolesModel RealmManagement { get; set; } = new RolesModel();

    /// <summary>
    /// get/set - Realm management claims.
    /// </summary>
    [JsonPropertyName("mmi-app")]
    public RolesModel App { get; set; } = new RolesModel();

    /// <summary>
    /// get/set - Realm management claims.
    /// </summary>
    [JsonPropertyName("mmi-service-account")]
    public RolesModel ServiceAccount { get; set; } = new RolesModel();

    /// <summary>
    /// get/set - Realm management claims.
    /// </summary>
    [JsonPropertyName("account")]
    public RolesModel Account { get; set; } = new RolesModel();
    #endregion
}
