using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

/// <summary>
/// Logging in must never change a user's subscriptions.
/// Saving a stale admin user form must be rejected instead of changing subscriptions made after it was loaded.
/// </summary>
public class UserServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public UserServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("user-service");
            services.AddPrincipalForRole("editor");
            services.AddMockSingleton<ILogger<UserService>>();
            services.AddSingleton<IUserService, UserService>();
        });
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Seed alice, subscribed to report 1 and unsubscribed from report 2.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static User SeedUser(TNOContext context)
    {
        var owner = new User("owner", "owner@test.com") { Id = 1 };
        var alice = new User("alice", "alice@test.com", "alice-key") { Id = 2, Status = UserStatus.Preapproved };
        context.AddRange(owner, alice);

        var template = new ReportTemplate("template", ReportType.Content, "subject", "body") { Id = 1 };
        context.Add(template);
        context.AddRange(
            new Report(1, "report-1", template.Id, owner.Id),
            new Report(2, "report-2", template.Id, owner.Id),
            new Report(3, "report-3", template.Id, owner.Id));
        context.AddRange(
            new UserReport(alice.Id, 1, true),
            new UserReport(alice.Id, 2, false));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return alice;
    }

    private static Dictionary<int, UserReport> GetSubscriptions(TNOContext context, int userId)
    {
        context.ChangeTracker.Clear();
        return context.UserReports.Where(ur => ur.UserId == userId).ToDictionary(ur => ur.ReportId);
    }
    #endregion

    #region Methods
    [Fact]
    public void UpdateLoginAndSave_LeavesSubscriptionsUntouched()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        SeedUser(context);

        // Mirror a login: the user is loaded by key and the last login is tracked.
        var user = service.FindByUserKey("alice-key")!;
        var lastLoginOn = DateTime.UtcNow;
        user.LastLoginOn = lastLoginOn;

        // Act
        service.UpdateLoginAndSave(user);
        var subscriptions = GetSubscriptions(context, user.Id);

        // Assert
        Assert.Equal(lastLoginOn, context.Users.Single(u => u.Id == user.Id).LastLoginOn);
        Assert.True(subscriptions[1].IsSubscribed);
        Assert.False(subscriptions[2].IsSubscribed);
        Assert.All(subscriptions.Values, s => Assert.Equal(0, s.Version));
    }

    [Fact]
    public void UpdateLoginAndSave_SavesLoginLocations()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        SeedUser(context);

        // Mirror a login capturing its location in the preferences of the tracked user.
        var user = service.FindByUserKey("alice-key")!;
        user.Preferences = System.Text.Json.JsonDocument.Parse("""{"locations":[{"key":"device-1","ipv4":"10.0.0.1"}]}""");

        // Act
        service.UpdateLoginAndSave(user);
        context.ChangeTracker.Clear();

        // Assert
        var saved = context.Users.Single(u => u.Id == user.Id);
        Assert.Equal("device-1", saved.Preferences.RootElement.GetProperty("locations")[0].GetProperty("key").GetString());
    }

    [Fact]
    public void UpdateAccountAndSave_LeavesSubscriptionsUntouched()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        SeedUser(context);

        // Mirror activating a preapproved account on its first login.
        var user = service.FindByEmail("alice@test.com").Single();
        user.Status = UserStatus.Approved;
        user.Roles = "[subscriber]";

        // Act
        service.UpdateAccountAndSave(user);
        var subscriptions = GetSubscriptions(context, user.Id);

        // Assert
        var saved = context.Users.Single(u => u.Id == user.Id);
        Assert.Equal(UserStatus.Approved, saved.Status);
        Assert.Equal("[subscriber]", saved.Roles);
        Assert.True(subscriptions[1].IsSubscribed);
        Assert.False(subscriptions[2].IsSubscribed);
        Assert.All(subscriptions.Values, s => Assert.Equal(0, s.Version));
    }

    [Fact]
    public void UpdateAndSave_StaleSubscription_ThrowsConcurrencyAndDoesNotResubscribe()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var alice = SeedUser(context);

        // Alice is unsubscribed from report 1 after the stale form loaded her.
        var subscription = context.UserReports.Single(ur => ur.UserId == alice.Id && ur.ReportId == 1);
        subscription.IsSubscribed = false;
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var stale = new User(alice.Username, alice.Email, alice.Key) { Id = alice.Id };
        stale.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 1, true) { ExpectedVersion = 0 });
        stale.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 2, false) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        Assert.False(GetSubscriptions(context, alice.Id)[1].IsSubscribed);
    }

    [Fact]
    public void UpdateAndSave_SubscriptionNotLoaded_ThrowsConcurrency()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var alice = SeedUser(context);

        // The form only loaded report 1, the subscription to report 2 was added after it loaded.
        var stale = new User(alice.Username, alice.Email, alice.Key) { Id = alice.Id };
        stale.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 1, true) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
    }

    [Fact]
    public void UpdateAndSave_CurrentSubscriptions_AppliesSubscribedValues()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IUserService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var alice = SeedUser(context);

        // Unsubscribe report 1, leave report 2 unsubscribed, subscribe to report 3.
        var current = new User(alice.Username, alice.Email, alice.Key) { Id = alice.Id };
        current.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 1, false) { ExpectedVersion = 0 });
        current.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 2, false) { ExpectedVersion = 0 });
        current.ReportSubscriptionsManyToMany.Add(new UserReport(alice.Id, 3, true));

        // Act
        service.UpdateAndSave(current);
        var subscriptions = GetSubscriptions(context, alice.Id);

        // Assert
        Assert.Equal(3, subscriptions.Count);
        Assert.False(subscriptions[1].IsSubscribed);
        Assert.False(subscriptions[2].IsSubscribed);
        Assert.True(subscriptions[3].IsSubscribed);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        var context = helper.Provider.GetRequiredService<TNOContext>();
        context.EnsureDeleted();
        context.Dispose();
    }
    #endregion
}
