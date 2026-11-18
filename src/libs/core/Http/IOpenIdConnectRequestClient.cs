using TNO.Core.Http.Configuration;

namespace TNO.Core.Http;

/// <summary>
/// IOpenIdConnectRequestClient interface, for OIDC client that handles AJAX requests.
/// </summary>
public interface IOpenIdConnectRequestClient : IHttpRequestClient
{
    AuthClientOptions AuthClientOptions { get; }
    OpenIdConnectOptions OpenIdConnectOptions { get; }
    Task<Models.OpenIdConnectModel?> GetOpenIdConnectEndpoints();
    Task<string> RequestAccessToken();
    Task<HttpResponseMessage> RequestToken();
    Task<HttpResponseMessage> RefreshToken(string refreshToken);
}
