using System.ComponentModel.DataAnnotations;

namespace TNO.Keycloak;

/// <summary>
/// ClientRole enum, provides the possible Keycloak client roles.
/// </summary>
public enum ClientRole
{
    /// <summary>
    /// Administrator role.
    /// </summary>
    [Display(Name = "administrator")]
    Administrator,
    /// <summary>
    /// Editor role.
    /// </summary>
    [Display(Name = "editor")]
    Editor,
    /// <summary>
    /// Subscriber role.
    /// </summary>
    [Display(Name = "subscriber")]
    Subscriber
}
