using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Config;
using TNO.API.Keycloak;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;
using Xunit;

namespace TNO.Test.API;

public class LogoutLocationTest
{
    private readonly Mock<IUserService> _users = new();
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "authenticated-user")], "test"));

    private KeycloakHelper Helper() => new(Mock.Of<IKeycloakService>(), _users.Object,
        Options.Create(new KeycloakOptions()), Mock.Of<ILogger<KeycloakHelper>>());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MissingDeviceKeyCannotSilentlySkipLogout(string? key)
    {
        Assert.ThrowsAny<ArgumentException>(() => Helper().RemoveLocation(_principal, key));
        _users.VerifyNoOtherCalls();
    }

    [Fact]
    public void LogoutRemovesOnlySelectedDeviceFromAuthenticatedUser()
    {
        var user = new User("user", "user@test.com", "authenticated-user")
        {
            Preferences = JsonDocument.Parse("""{"theme":"dark","locations":[{"Key":"device-a"},{"Key":"device-b"}]}"""),
        };
        _users.Setup(u => u.FindByUserKey("authenticated-user")).Returns(user);
        _users.Setup(u => u.UpdateLoginAndSave(user)).Returns(user);

        Helper().RemoveLocation(_principal, "device-a");

        Assert.Equal("dark", user.Preferences.RootElement.GetProperty("theme").GetString());
        var locations = user.Preferences.RootElement.GetProperty("locations");
        Assert.Equal(1, locations.GetArrayLength());
        Assert.Equal("device-b", locations[0].GetProperty("Key").GetString());
        _users.Verify(u => u.UpdateLoginAndSave(user), Times.Once);
        _users.Verify(u => u.FindByUserKey("authenticated-user"), Times.Once);
        _users.VerifyNoOtherCalls();
    }
}
