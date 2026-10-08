using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

/// <summary>
/// Notification subscriptions must never be deleted by a notification update.
/// Saving a stale admin notification form must be rejected instead of changing subscriptions made after it was loaded.
/// </summary>
public class NotificationServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public NotificationServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("notification-service");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.Configure<ElasticOptions>(o => { });
            services.Configure<JsonSerializerOptions>(o => { });
            services.AddMockSingleton<ITNOElasticClient>();
            services.AddMockSingleton<ISettingService>();
            services.AddMockSingleton<ILogger<NotificationService>>();
            services.AddSingleton<INotificationService, NotificationService>();
        });
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Seed a notification that alice and bob are subscribed to.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static Notification SeedNotification(TNOContext context)
    {
        var owner = new User("owner", "owner@test.com") { Id = 1 };
        var alice = new User("alice", "alice@test.com") { Id = 2 };
        var bob = new User("bob", "bob@test.com") { Id = 3 };
        var carol = new User("carol", "carol@test.com") { Id = 4 };
        context.AddRange(owner, alice, bob, carol);

        var template = new NotificationTemplate(1, "template", "subject", "body");
        context.Add(template);

        var notification = new Notification(1, "notification", NotificationType.Email, owner.Id, template.Id);
        context.Add(notification);
        context.AddRange(
            new UserNotification(alice.Id, notification.Id, true),
            new UserNotification(bob.Id, notification.Id, true));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return notification;
    }

    private static Notification Copy(Notification notification)
    {
        return new Notification(notification.Id, notification.Name, notification.NotificationType, notification.OwnerId ?? 0, notification.TemplateId);
    }

    private static Dictionary<int, UserNotification> GetSubscriptions(TNOContext context, int notificationId)
    {
        context.ChangeTracker.Clear();
        return context.UserNotifications.Where(un => un.NotificationId == notificationId).ToDictionary(un => un.UserId);
    }
    #endregion

    #region Methods
    [Fact]
    public void Update_SubscriberNotLoaded_ThrowsConcurrencyAndDeletesNothing()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<INotificationService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var notification = SeedNotification(context);

        // A stale client only loaded alice, bob was subscribed after it loaded.
        var stale = Copy(notification);
        stale.SubscribersManyToMany.Add(new UserNotification(2, notification.Id, true) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        var subscriptions = GetSubscriptions(context, notification.Id);
        Assert.Equal(2, subscriptions.Count);
        Assert.All(subscriptions.Values, s => Assert.True(s.IsSubscribed));
    }

    [Fact]
    public void Update_StaleSubscriberVersion_ThrowsConcurrencyAndDoesNotResubscribe()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<INotificationService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var notification = SeedNotification(context);

        // An admin unsubscribes bob after the stale client loaded the notification.
        var bob = context.UserNotifications.Single(un => un.NotificationId == notification.Id && un.UserId == 3);
        bob.IsSubscribed = false;
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var stale = Copy(notification);
        stale.SubscribersManyToMany.Add(new UserNotification(2, notification.Id, true) { ExpectedVersion = 0 });
        stale.SubscribersManyToMany.Add(new UserNotification(3, notification.Id, true) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        Assert.False(GetSubscriptions(context, notification.Id)[3].IsSubscribed);
    }

    [Fact]
    public void Update_CurrentSubscribers_UnsubscribesWithoutDeletingAndAddsNewSubscriber()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<INotificationService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var notification = SeedNotification(context);

        // The client loaded alice and bob, unsubscribes bob and adds carol.
        var current = Copy(notification);
        current.SubscribersManyToMany.Add(new UserNotification(2, notification.Id, true) { ExpectedVersion = 0 });
        current.SubscribersManyToMany.Add(new UserNotification(3, notification.Id, false) { ExpectedVersion = 0 });
        current.SubscribersManyToMany.Add(new UserNotification(4, notification.Id, true));

        // Act
        service.UpdateAndSave(current);
        var subscriptions = GetSubscriptions(context, notification.Id);

        // Assert
        Assert.Equal(3, subscriptions.Count);
        Assert.True(subscriptions[2].IsSubscribed);
        Assert.False(subscriptions[3].IsSubscribed);
        Assert.True(subscriptions[4].IsSubscribed);
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
