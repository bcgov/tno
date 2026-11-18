using Microsoft.AspNetCore.Http;

namespace TNO.Core.Http
{
    public interface IProxyRequestClient : IDisposable
    {
        Task<HttpResponseMessage> ProxySendAsync(HttpRequest request, string url, HttpMethod? method = null, HttpContent? content = null);
        Task<HttpResponseMessage> ProxyGetAsync(HttpRequest request, string url);
        Task<HttpResponseMessage> ProxyPostAsync(HttpRequest request, string url, HttpContent? content = null);
        Task<HttpResponseMessage> ProxyPutAsync(HttpRequest request, string url, HttpContent? content = null);
        Task<HttpResponseMessage> ProxyDeleteAsync(HttpRequest request, string url, HttpContent? content = null);
    }
}
