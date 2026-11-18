using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Core.Exceptions;
using TNO.Core.Http;
using TNO.Core.Http.Models;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace TNO.Test.Services;

public class ChesServiceTest
{
    private const string AuthUrl = "https://login.test/token";
    private const string HostUri = "https://ches.test/api/v1";

    private static ChesService Service(Mock<IHttpRequestClient> client) => new(
        MsOptions.Create(new ChesOptions { AuthUrl = AuthUrl, HostUri = HostUri, From = "mmi@test.com", EmailEnabled = true, EmailAuthorized = true }),
        new ClaimsPrincipal(new ClaimsIdentity()),
        client.Object,
        new JwtSecurityTokenHandler(),
        NullLogger<IChesService>.Instance);

    private static TokenModel Token() => new()
    {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(5))),
    };

    private static EmailMergeModel Email(int index) => new("mmi@test.com", [new EmailContextModel([$"user{index}@test.com"], [], DateTime.UtcNow)], "subject", "body");

    private static HttpClientRequestException Failure(HttpStatusCode status, string body, string mediaType) => new(new HttpResponseMessage(status)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Post, AuthUrl),
        Content = new StringContent(body, new MediaTypeHeaderValue(mediaType)),
    });

    [Fact]
    public async Task ConcurrentEmailsShareOneTokenRequest()
    {
        var client = new Mock<IHttpRequestClient>();
        var tokenRequests = 0;
        client.Setup(c => c.SendAsync<TokenModel>(AuthUrl, HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<HttpContent?>(), null))
            .Returns(async () =>
            {
                Interlocked.Increment(ref tokenRequests);
                await Task.Delay(50);
                return Token();
            });
        client.Setup(c => c.SendJsonAsync<EmailResponseModel, IEmailMerge>($"{HostUri}/emailMerge", HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<IEmailMerge?>(), null))
            .ReturnsAsync(new EmailResponseModel());
        var service = Service(client);

        await Task.WhenAll(Enumerable.Range(0, 25).Select(i => service.SendEmailAsync(Email(i))));

        Assert.Equal(1, tokenRequests);
        client.Verify(c => c.SendJsonAsync<EmailResponseModel, IEmailMerge>($"{HostUri}/emailMerge", HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<IEmailMerge?>(), null), Times.Exactly(25));
    }

    [Fact]
    public async Task TokenFailureWithAnHtmlBodyIsAChesException()
    {
        // The gateway in front of the token endpoint answers a timeout with HTML, not JSON.
        var client = new Mock<IHttpRequestClient>();
        client.Setup(c => c.SendAsync<TokenModel>(AuthUrl, HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<HttpContent?>(), null))
            .ThrowsAsync(Failure(HttpStatusCode.RequestTimeout, "<html><body><h1>408 Request Time-out</h1></body></html>", "text/html"));
        var service = Service(client);

        var ex = await Assert.ThrowsAsync<ChesException>(() => service.SendEmailAsync(Email(1)));

        Assert.IsType<HttpClientRequestException>(ex.InnerException);
        client.Verify(c => c.DeserializeAsync<ErrorResponseModel>(It.IsAny<HttpResponseMessage>()), Times.Never);
    }

    [Fact]
    public async Task TokenFailureWithoutAResponseIsAChesException()
    {
        var client = new Mock<IHttpRequestClient>();
        client.Setup(c => c.SendAsync<TokenModel>(AuthUrl, HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<HttpContent?>(), null))
            .ThrowsAsync(new TaskCanceledException("The request timed out"));
        var service = Service(client);

        var ex = await Assert.ThrowsAsync<ChesException>(() => service.SendEmailAsync(Email(1)));

        Assert.IsType<TaskCanceledException>(ex.InnerException);
    }

    [Fact]
    public async Task TokenIsRequestedAgainAfterAFailure()
    {
        var client = new Mock<IHttpRequestClient>();
        client.SetupSequence(c => c.SendAsync<TokenModel>(AuthUrl, HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<HttpContent?>(), null))
            .ThrowsAsync(Failure(HttpStatusCode.RequestTimeout, "<html></html>", "text/html"))
            .ReturnsAsync(Token());
        client.Setup(c => c.SendJsonAsync<EmailResponseModel, IEmailMerge>($"{HostUri}/emailMerge", HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<IEmailMerge?>(), null))
            .ReturnsAsync(new EmailResponseModel());
        var service = Service(client);

        await Assert.ThrowsAsync<ChesException>(() => service.SendEmailAsync(Email(1)));
        await service.SendEmailAsync(Email(2));

        client.Verify(c => c.SendJsonAsync<EmailResponseModel, IEmailMerge>($"{HostUri}/emailMerge", HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<IEmailMerge?>(), null), Times.Once);
    }

    [Fact]
    public async Task ErrorDetailIsReadFromAJsonBody()
    {
        var client = new Mock<IHttpRequestClient>();
        client.Setup(c => c.SendAsync<TokenModel>(AuthUrl, HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<HttpContent?>(), null)).ReturnsAsync(Token());
        client.Setup(c => c.SendJsonAsync<EmailResponseModel, IEmailMerge>($"{HostUri}/emailMerge", HttpMethod.Post, It.IsAny<HttpRequestHeaders>(), It.IsAny<IEmailMerge?>(), null))
            .ThrowsAsync(Failure(HttpStatusCode.ServiceUnavailable, "{\"type\":\"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/503\",\"title\":\"Service Unavailable\",\"status\":503,\"detail\":\"Server is shutting down\"}", "application/problem+json"));
        var service = Service(client);

        var ex = await Assert.ThrowsAsync<ChesException>(() => service.SendEmailAsync(Email(1)));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.Contains("Server is shutting down", ex.Detail);
    }
}
