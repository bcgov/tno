using System;
using System.ComponentModel.DataAnnotations;

namespace TNO.CSS.Config;

public class CssOptions
{
    #region Properties
    /// <summary>
    /// get/set - The CSS 'ApiUrl' URL.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'ApiUrl' is required.")]
    public Uri ApiUrl { get; set; } = new Uri("https://api.loginproxy.gov.bc.ca");

    /// <summary>
    /// get/set - The CSS 'Authority' URL.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'Authority' is required.")]
    public Uri Authority { get; set; } = new Uri("https://loginproxy.gov.bc.ca");

    /// <summary>
    /// get/set - The path to the access token endpoint.
    /// </summary>
    public string TokenPath { get; set; } = "/auth/realms/standard/protocol/openid-connect/token";

    /// <summary>
    /// get/set - The CSS client 'ClientId' is the Keycloak client ID.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'ClientId' is required.")]
    public string? ClientId { get; set; }

    /// <summary>
    /// get/set - The CSS client 'Secret'.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'Secret' is required.")]
    public string? Secret { get; set; }
    #endregion
}
