using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ResponseCaptureHandler : DelegatingHandler
{
    private readonly string _path;

    public ResponseCaptureHandler(string path) : base(new HttpClientHandler())
    {
        _path = path;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(_path, body, cancellationToken).ConfigureAwait(false);
            response.Content = new StringContent(body, Encoding.UTF8, response.Content.Headers?.ContentType?.MediaType ?? "application/json");
        }
        catch
        {
        }

        return response;
    }
}
