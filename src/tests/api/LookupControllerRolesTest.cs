using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Editor.Controllers;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.Core.Http;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Keycloak.Models;
using Xunit;

namespace TNO.Test.API;

public class LookupControllerRolesTest
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private readonly Mock<IKeycloakService> _keycloak = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private sealed class NoHolidays : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
    }

    private LookupController CreateController()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(new NoHolidays()));
        return new LookupController(
            Mock.Of<IHttpRequestClient>(), factory.Object, _cache,
            Mock.Of<IActionService>(), Mock.Of<ITopicService>(), Mock.Of<IMediaTypeService>(), Mock.Of<ISourceService>(),
            Mock.Of<ILicenseService>(), Mock.Of<IIngestTypeService>(), Mock.Of<ISeriesService>(), Mock.Of<IContributorService>(),
            Mock.Of<IMetricService>(), Mock.Of<ITagService>(), Mock.Of<ITonePoolService>(), Mock.Of<IUserService>(),
            Mock.Of<IDataLocationService>(), Mock.Of<ISettingService>(), _keycloak.Object,
            Options.Create(new TNO.API.Config.KeycloakOptions { ClientId = ClientId }),
            Options.Create(new JsonSerializerOptions()), NullLogger<LookupController>.Instance, Mock.Of<IOrganizationService>());
    }

    private async Task<string[]> GetRolesAsync()
    {
        var result = (JsonResult)await CreateController().FindAllAsync();
        return ((LookupModel)result.Value!).Roles.Select(r => r.Name).ToArray();
    }

    private void SetupRoles(params string[] names)
        => _keycloak.Setup(k => k.GetRolesAsync(ClientId)).ReturnsAsync(names.Select(n => new RoleModel { Name = n }).ToArray());

    /// <summary>
    /// Expire only the fresh roles, as the passing of time would.
    /// </summary>
    private void ExpireRoles() => _cache.Remove("lookup:keycloak-roles");

    [Fact]
    public async Task Lookups_FetchRolesOnce()
    {
        SetupRoles("editor");

        Assert.Equal(["editor"], await GetRolesAsync());
        Assert.Equal(["editor"], await GetRolesAsync());

        _keycloak.Verify(k => k.GetRolesAsync(ClientId), Times.Once);
    }

    [Fact]
    public async Task ExpiredRoles_AreFetchedAgain()
    {
        SetupRoles("editor");
        await GetRolesAsync();

        SetupRoles("editor", "subscriber");
        ExpireRoles();

        Assert.Equal(["editor", "subscriber"], await GetRolesAsync());
    }

    [Fact]
    public async Task KeycloakFailure_ServesLastRoles_AndWaitsToRetry()
    {
        SetupRoles("editor");
        await GetRolesAsync();

        _keycloak.Invocations.Clear();
        _keycloak.Setup(k => k.GetRolesAsync(ClientId)).ThrowsAsync(new HttpRequestException("reset"));
        ExpireRoles();

        Assert.Equal(["editor"], await GetRolesAsync());
        Assert.Equal(["editor"], await GetRolesAsync());
        _keycloak.Verify(k => k.GetRolesAsync(ClientId), Times.Once);
    }

    [Fact]
    public async Task KeycloakFailure_WithNothingCached_Throws()
    {
        _keycloak.Setup(k => k.GetRolesAsync(ClientId)).ThrowsAsync(new HttpRequestException("reset"));

        await Assert.ThrowsAsync<HttpRequestException>(GetRolesAsync);
    }

    [Fact]
    public async Task EmptyRoles_AreNotCached()
    {
        SetupRoles();
        Assert.Empty(await GetRolesAsync());

        SetupRoles("editor");

        Assert.Equal(["editor"], await GetRolesAsync());
    }
}
