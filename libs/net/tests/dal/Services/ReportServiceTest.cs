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
/// Report subscriptions must never be deleted by a report update.
/// A client holding a stale subscriber list must not be able to remove subscriptions added elsewhere.
/// </summary>
public class ReportServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public ReportServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("report-service");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.Configure<ElasticOptions>(o => { });
            services.Configure<JsonSerializerOptions>(o => { });
            services.AddMockSingleton<ITNOElasticClient>();
            services.AddMockSingleton<IReportInstanceService>();
            services.AddMockSingleton<ILogger<ReportService>>();
            services.AddSingleton<IReportService, ReportService>();
        });
    }
    #endregion

    #region Helpers
    private Report SeedReport(TNOContext context)
    {
        var owner = new User("owner", "owner@test.com") { Id = 1 };
        var alice = new User("alice", "alice@test.com") { Id = 2 };
        var bob = new User("bob", "bob@test.com") { Id = 3 };
        var carol = new User("carol", "carol@test.com") { Id = 4 };
        context.AddRange(owner, alice, bob, carol);

        var template = new ReportTemplate("template", ReportType.Content, "subject", "body") { Id = 1 };
        context.Add(template);

        var report = new Report(1, "report", template.Id, owner.Id);
        context.Add(report);
        context.AddRange(
            new UserReport(alice.Id, report.Id, true),
            new UserReport(bob.Id, report.Id, true));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return report;
    }
    #endregion

    #region Methods
    [Fact]
    public void Update_SubscriberNotLoaded_ThrowsConcurrencyAndChangesNothing()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        // A stale client only loaded alice, bob was subscribed after it loaded.
        var stale = new Report(report.Id, report.Name, report.TemplateId, report.OwnerId);
        stale.SubscribersManyToMany.Add(new UserReport(2, report.Id, true) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        context.ChangeTracker.Clear();
        var subscriptions = context.UserReports.Where(ur => ur.ReportId == report.Id).ToArray();
        Assert.Equal(2, subscriptions.Length);
        Assert.All(subscriptions, s => Assert.True(s.IsSubscribed));
    }

    [Fact]
    public void Update_StaleSubscriberVersion_ThrowsConcurrencyAndDoesNotResubscribe()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        // An admin unsubscribes bob after the stale client loaded the report.
        var bob = context.UserReports.Single(ur => ur.ReportId == report.Id && ur.UserId == 3);
        bob.IsSubscribed = false;
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var stale = new Report(report.Id, report.Name, report.TemplateId, report.OwnerId);
        stale.SubscribersManyToMany.Add(new UserReport(2, report.Id, true) { ExpectedVersion = 0 });
        stale.SubscribersManyToMany.Add(new UserReport(3, report.Id, true) { ExpectedVersion = 0 });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        context.ChangeTracker.Clear();
        Assert.False(context.UserReports.Single(ur => ur.ReportId == report.Id && ur.UserId == 3).IsSubscribed);
    }

    [Fact]
    public void Update_CurrentSubscribers_AppliesChangesAndAddsNewSubscriber()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        // The client loaded alice and bob, unsubscribes bob and adds carol.
        var current = new Report(report.Id, report.Name, report.TemplateId, report.OwnerId);
        current.SubscribersManyToMany.Add(new UserReport(2, report.Id, true) { ExpectedVersion = 0 });
        current.SubscribersManyToMany.Add(new UserReport(3, report.Id, false) { ExpectedVersion = 0 });
        current.SubscribersManyToMany.Add(new UserReport(4, report.Id, true));

        // Act
        service.UpdateAndSave(current);
        context.ChangeTracker.Clear();
        var subscriptions = context.UserReports.Where(ur => ur.ReportId == report.Id).ToDictionary(ur => ur.UserId);

        // Assert
        Assert.Equal(3, subscriptions.Count);
        Assert.True(subscriptions[2].IsSubscribed);
        Assert.False(subscriptions[3].IsSubscribed);
        Assert.True(subscriptions[4].IsSubscribed);
    }

    [Fact]
    public void Update_WithoutSubscribers_LeavesSubscriptionsUntouched()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        var stale = new Report(report.Id, "renamed", report.TemplateId, report.OwnerId);

        // Act
        service.Update(stale, false);
        context.CommitTransaction();
        context.ChangeTracker.Clear();
        var subscriptions = context.UserReports.Where(ur => ur.ReportId == report.Id).ToArray();

        // Assert
        Assert.Equal("renamed", context.Reports.Single(r => r.Id == report.Id).Name);
        Assert.Equal(2, subscriptions.Length);
        Assert.All(subscriptions, s => Assert.True(s.IsSubscribed));
    }

    [Fact]
    public void UpdateSubscribersAndSave_OnlyTouchesSubmittedRows()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        // Unsubscribe bob, add carol, say nothing about alice.
        var changes = new[]
        {
            new UserReport(3, report.Id, false),
            new UserReport(4, report.Id, true, ReportDistributionFormat.LinkOnly, EmailSentTo.BCC),
        };

        // Act
        var result = service.UpdateSubscribersAndSave(report.Id, changes);
        context.ChangeTracker.Clear();
        var subscriptions = context.UserReports.Where(ur => ur.ReportId == report.Id).OrderBy(ur => ur.UserId).ToDictionary(ur => ur.UserId);

        // Assert
        Assert.Equal(3, subscriptions.Count);
        Assert.True(subscriptions[2].IsSubscribed);
        Assert.False(subscriptions[3].IsSubscribed);
        Assert.True(subscriptions[4].IsSubscribed);
        Assert.Equal(ReportDistributionFormat.LinkOnly, subscriptions[4].Format);
        Assert.Equal(EmailSentTo.BCC, subscriptions[4].SendTo);
        Assert.Equal(3, result.SubscribersManyToMany.Count);
    }

    [Fact]
    public void UpdateSubscribersAndSave_UnknownUser_Throws()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IReportService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var report = SeedReport(context);

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.UpdateSubscribersAndSave(report.Id, new[] { new UserReport(99, report.Id, true) }));
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
