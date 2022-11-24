using System.Net.Http;
using System.Threading.Tasks;
using TNO.Core.Http;
using TNO.CSS.Config;

namespace TNO.CSS;

/// <summary>
/// ICssClient interface, provides a wrapper API to communicate with CSS API.
/// <ref href="https://api.loginproxy.gov.bc.ca/openapi/swagger#/"/>
/// </summary>
public interface ICssClient : IHttpRequestClient
{
    #region Properties
    CssOptions ClientOptions { get; }
    #endregion

    #region Methods
    Task<HttpResponseMessage> RequestTokenAsync();
    Task<HttpResponseMessage> RefreshTokenAsync(string refreshToken);
    #endregion
}
