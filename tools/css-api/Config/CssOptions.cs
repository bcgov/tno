namespace TNO.CSS.API.Config;

/// <summary>
/// CssOptions class, provides a way to configure Keycloak.
/// </summary>
public class CssOptions : TNO.Keycloak.Configuration.KeycloakOptions
{
    #region Properties
    /// <summary>
    /// get/set - The keycloak client id.
    /// </summary>
    public Guid? ClientId { get; set; }
    #endregion
}
