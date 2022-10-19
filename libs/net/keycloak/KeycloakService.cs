using System;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.Core.Http;

namespace TNO.Keycloak;

/// <summary>
/// KeycloakService class, provides a service for sending HTTP requests to the keycloak admin API.
///     - https://www.keycloak.org/docs-api/5.0/rest-api/index.html#_overview
/// </summary>
public partial class KeycloakService : IKeycloakService
{
    #region Variables
    private const string AUTH_URL = "/auth/realms/";
    private const string ADMIN_URL = "/auth/admin/realms/";
    private readonly IOpenIdConnectRequestClient _client;
    #endregion

    #region Properties
    /// <summary>
    /// get - The configuration options for keycloak.
    /// </summary>
    /// <value></value>
    public Configuration.KeycloakOptions Options { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KeycloakAdmin class, initializes it with the specified arguments.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    public KeycloakService(IOpenIdConnectRequestClient client, IOptions<Configuration.KeycloakOptions> options)
    {
        this.Options = options.Value;
        this.Options.Validate(); // TODO: Figure out how to automatically validate.
        _client = client;
        _client.AuthClientOptions.Authority = GetAuthorityUrl().AbsoluteUri;
        _client.AuthClientOptions.Audience = this.Options.Audience;
        _client.AuthClientOptions.Secret = this.Options.Secret;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the base Authority URL.
    /// </summary>
    /// <returns></returns>
    private Uri GetAuthorityUrl()
    {
        return new Uri(this.Options.Authority).Append(AUTH_URL, this.Options.Realm);
    }

    /// <summary>
    /// Get the base Authority URL.
    /// </summary>
    /// <returns></returns>
    private Uri GetBaseUrl()
    {
        return new Uri(this.Options.Authority).Append(ADMIN_URL, this.Options.Realm);
    }
    #endregion
}
