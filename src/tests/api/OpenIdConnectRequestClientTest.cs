using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TNO.Core.Exceptions;
using TNO.Core.Http;
using TNO.Core.Http.Configuration;
using Xunit;

namespace TNO.Test.API;

public class OpenIdConnectRequestClientTest
{
    private const string TokenUrl = "https://keycloak.test/token";
    private const string ResourceUrl = "https://keycloak.test/admin/roles";

    /// <summary>
    /// Stands in for keycloak, issuing the queued tokens and recording what it was asked for.
    /// </summary>
    private sealed class FakeKeycloak : HttpMessageHandler
    {
        private int _issued;
        public readonly List<string> Grants = [];
        public readonly List<string?> ResourceTokens = [];
        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan? RefreshLifetime { get; set; }
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
        public HashSet<string> RejectedGrants { get; } = [];
        public HttpStatusCode ResourceStatus { get; set; } = HttpStatusCode.OK;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsoluteUri != TokenUrl)
            {
                lock (ResourceTokens) ResourceTokens.Add(request.Headers.GetValues("Authorization").Single());
                return new HttpResponseMessage(ResourceStatus) { Content = new StringContent("{}") };
            }

            var form = await request.Content!.ReadAsStringAsync(cancellationToken);
            var grant = form.Split('&').Single(p => p.StartsWith("grant_type=", StringComparison.Ordinal))["grant_type=".Length..];
            lock (Grants) Grants.Add(grant);
            await Task.Delay(Delay, cancellationToken);
            if (RejectedGrants.Contains(grant)) return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{}") };

            var id = Interlocked.Increment(ref _issued);
            var body = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["access_token"] = CreateJwt($"access-{id}", TokenLifetime),
                ["refresh_token"] = RefreshLifetime.HasValue ? CreateJwt($"refresh-{id}", RefreshLifetime.Value) : null,
            });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }

    private static string CreateJwt(string id, TimeSpan lifetime)
    {
        var claims = new[] { new System.Security.Claims.Claim("jti", id) };
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.Add(lifetime)));
    }

    /// <summary>
    /// Build the client the way the API registers it, scoped per request.
    /// </summary>
    private static ServiceProvider CreateProvider(FakeKeycloak keycloak, bool shareToken = true)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(keycloak, false));
        var services = new ServiceCollection()
            .AddSingleton(factory.Object)
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddScoped<JwtSecurityTokenHandler>()
            .AddScoped<IOpenIdConnectRequestClient, OpenIdConnectRequestClient>();
        services.AddOptions();
        services.Configure<AuthClientOptions>(o => { o.Authority = "https://keycloak.test"; o.Audience = "mmi-service-account"; o.Secret = "secret"; });
        services.Configure<OpenIdConnectOptions>(o => o.Token = TokenUrl);
        if (shareToken) services.AddSingleton<OpenIdConnectTokenCache>();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Send a request in its own scope, as an API request does.
    /// </summary>
    private static async Task<HttpStatusCode> SendAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IOpenIdConnectRequestClient>();
        return (await client.GetAsync(ResourceUrl)).StatusCode;
    }

    [Fact]
    public async Task RequestsInDifferentScopes_ReuseOneToken()
    {
        var keycloak = new FakeKeycloak();
        using var provider = CreateProvider(keycloak);

        await SendAsync(provider);
        await SendAsync(provider);
        await SendAsync(provider);

        Assert.Equal(["client_credentials"], keycloak.Grants);
        Assert.Equal(3, keycloak.ResourceTokens.Count);
        Assert.Single(keycloak.ResourceTokens.Distinct());
    }

    [Fact]
    public async Task ConcurrentRequests_ShareOneRenewal()
    {
        var keycloak = new FakeKeycloak { Delay = TimeSpan.FromMilliseconds(100) };
        using var provider = CreateProvider(keycloak);

        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => SendAsync(provider)));

        Assert.Equal(["client_credentials"], keycloak.Grants);
        Assert.Equal(20, keycloak.ResourceTokens.Count);
    }

    [Fact]
    public async Task WithoutSharedCache_EachClientKeepsItsOwnToken()
    {
        var keycloak = new FakeKeycloak();
        using var provider = CreateProvider(keycloak, shareToken: false);

        await SendAsync(provider);
        await SendAsync(provider);

        Assert.Equal(2, keycloak.Grants.Count);
    }

    [Fact]
    public async Task TokenCloseToExpiring_IsRefreshed()
    {
        var keycloak = new FakeKeycloak
        {
            TokenLifetime = TimeSpan.FromSeconds(10),
            RefreshLifetime = TimeSpan.FromMinutes(30),
        };
        using var provider = CreateProvider(keycloak);

        await SendAsync(provider);
        await SendAsync(provider);

        Assert.Equal(["client_credentials", "refresh_token"], keycloak.Grants);
        Assert.NotEqual(keycloak.ResourceTokens[0], keycloak.ResourceTokens[1]);
    }

    [Fact]
    public async Task ExpiredToken_WithoutRefreshToken_RequestsNewToken()
    {
        var keycloak = new FakeKeycloak { TokenLifetime = TimeSpan.FromSeconds(-10) };
        using var provider = CreateProvider(keycloak);

        await SendAsync(provider);
        await SendAsync(provider);

        Assert.Equal(["client_credentials", "client_credentials"], keycloak.Grants);
    }

    [Fact]
    public async Task ExpiredRefreshToken_IsNotUsed()
    {
        var keycloak = new FakeKeycloak
        {
            TokenLifetime = TimeSpan.FromSeconds(-10),
            RefreshLifetime = TimeSpan.FromSeconds(-10),
        };
        using var provider = CreateProvider(keycloak);

        await SendAsync(provider);
        await SendAsync(provider);

        Assert.Equal(["client_credentials", "client_credentials"], keycloak.Grants);
    }

    [Fact]
    public async Task RejectedRefreshToken_FallsBackToNewToken()
    {
        var keycloak = new FakeKeycloak
        {
            TokenLifetime = TimeSpan.FromSeconds(-10),
            RefreshLifetime = TimeSpan.FromMinutes(30),
        };
        keycloak.RejectedGrants.Add("refresh_token");
        using var provider = CreateProvider(keycloak);

        await SendAsync(provider);
        var status = await SendAsync(provider);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(["client_credentials", "refresh_token", "client_credentials"], keycloak.Grants);
    }

    [Fact]
    public async Task FailedRenewal_KeepsTokenThatHasNotExpired()
    {
        var keycloak = new FakeKeycloak { TokenLifetime = TimeSpan.FromSeconds(10) };
        using var provider = CreateProvider(keycloak);
        await SendAsync(provider);

        keycloak.RejectedGrants.Add("client_credentials");
        var status = await SendAsync(provider);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(2, keycloak.ResourceTokens.Count);
        Assert.Single(keycloak.ResourceTokens.Distinct());
    }

    [Fact]
    public async Task FailedRenewal_WithExpiredToken_ThrowsThenRecovers()
    {
        var keycloak = new FakeKeycloak();
        keycloak.RejectedGrants.Add("client_credentials");
        using var provider = CreateProvider(keycloak);

        await Assert.ThrowsAsync<HttpClientRequestException>(() => SendAsync(provider));

        keycloak.RejectedGrants.Clear();
        Assert.Equal(HttpStatusCode.OK, await SendAsync(provider));
        Assert.Equal(2, keycloak.Grants.Count);
    }

    [Fact]
    public async Task UnauthorizedResponse_ReplacesTokenOnNextRequest()
    {
        var keycloak = new FakeKeycloak { ResourceStatus = HttpStatusCode.Unauthorized };
        using var provider = CreateProvider(keycloak);
        await SendAsync(provider);

        keycloak.ResourceStatus = HttpStatusCode.OK;
        await SendAsync(provider);

        Assert.Equal(["client_credentials", "client_credentials"], keycloak.Grants);
        Assert.NotEqual(keycloak.ResourceTokens[0], keycloak.ResourceTokens[1]);
    }
}
