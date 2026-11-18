using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Config;
using TNO.API.Keycloak;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;
using TNO.Keycloak.Models;
using Xunit;

namespace TNO.Test.API;

/// <summary>
/// One person signing in to the editor and the subscriber at the same moment saves two logins. The
/// second finds the user changed since it was read, which must not fail the sign-in.
/// </summary>
public class LoginConflictTest
{
    private const int UserId = 7;
    private static readonly Guid Key = Guid.NewGuid();
    private readonly Mock<IUserService> _users = new();
    private readonly Mock<IKeycloakService> _keycloak = new();

    private static User CreateUser(string preferences) => new("user", "user@test.com", Key.ToString())
    {
        Id = UserId,
        Roles = "[subscriber]",
        LastLoginOn = new DateTime(2026, 11, 12, 22, 47, 32, DateTimeKind.Utc),
        Preferences = JsonDocument.Parse(preferences),
    };

    private KeycloakHelper CreateHelper()
    {
        _keycloak.Setup(k => k.GetUserClientRolesAsync(Key, It.IsAny<Guid>()))
            .ReturnsAsync([new RoleModel { Name = "subscriber" }]);
        return new KeycloakHelper(_keycloak.Object, _users.Object,
            Options.Create(new KeycloakOptions { ClientId = Guid.NewGuid() }), Mock.Of<ILogger<KeycloakHelper>>());
    }

    private static ClaimsPrincipal Principal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Key.ToString())], "test"));

    [Fact]
    public async Task ConflictingLogin_IsSavedAgain_OnTheUserAsItIsNow()
    {
        var helper = CreateHelper();
        _users.Setup(u => u.FindByUserKey(Key.ToString())).Returns(CreateUser("{}"));
        // The other sign-in saved first; reading the user again returns its changes.
        var current = CreateUser("""{"theme":"dark"}""");
        _users.Setup(u => u.FindById(UserId)).Returns(current);
        var saved = new List<User>();
        var attempts = 0;
        _users.Setup(u => u.UpdateLoginAndSave(It.IsAny<User>())).Returns((User user) =>
        {
            if (++attempts == 1) throw new DbUpdateConcurrencyException("conflict");
            saved.Add(user);
            return user;
        });

        var before = DateTime.UtcNow;
        var (user, _) = await helper.ActivateAsync(Principal());

        Assert.Same(current, user);
        Assert.Single(saved);
        Assert.Same(current, saved[0]);
        Assert.True(saved[0].LastLoginOn >= before);
        Assert.Equal(Key.ToString(), saved[0].Key);
        // The preferences are those saved since, not the copy the first attempt read.
        Assert.Equal("dark", saved[0].Preferences.RootElement.GetProperty("theme").GetString());
        _users.Verify(u => u.ClearChangeTracker(), Times.Once);
    }

    [Fact]
    public async Task LoginThatKeepsConflicting_StillFails()
    {
        var helper = CreateHelper();
        _users.Setup(u => u.FindByUserKey(Key.ToString())).Returns(CreateUser("{}"));
        _users.Setup(u => u.FindById(UserId)).Returns(() => CreateUser("{}"));
        _users.Setup(u => u.UpdateLoginAndSave(It.IsAny<User>())).Throws(new DbUpdateConcurrencyException("conflict"));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => helper.ActivateAsync(Principal()));
        _users.Verify(u => u.UpdateLoginAndSave(It.IsAny<User>()), Times.Exactly(3));
    }
}
