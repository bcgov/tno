namespace TNO.API.Config;

/// <summary>
/// KeycloakOptions class, provides a way to configure Keycloak.
/// </summary>
public class KeycloakOptions
{
    #region Properties
    /// <summary>
    /// get/set - The keycloak client id.
    /// </summary>
    public Guid? ClientId { get; set; }
    #endregion
}
