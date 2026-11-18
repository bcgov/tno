using System;
using System.Text.Json.Serialization;

namespace TNO.Keycloak.Models;

/// <summary>
/// RolesModel class, provides a model to deserialize JSON values.
/// </summary>
public class RolesModel
{
    #region Properties
    /// <summary>
    /// get/set - An array of role names.
    /// </summary>
    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = Array.Empty<string>();
    #endregion
}
