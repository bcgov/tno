using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

/// <summary>
/// The short-lived store of AI section test runs: replaced per requestor and section, completed
/// once, removed on request, purged by age.
/// </summary>
public class ReportAITestServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public ReportAITestServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext();
            services.AddPrincipalForRole("subscriber");
            services.AddMockSingleton<ILogger<ReportAITestService>>();
            services.AddSingleton<IReportAITestService, ReportAITestService>();
        });
    }
    #endregion

    #region Methods
    private IReportAITestService Service => helper.Provider.GetRequiredService<IReportAITestService>();
    private TNOContext Context => helper.Provider.GetRequiredService<TNOContext>();

    private static ReportAITest Run(int requestorId = 400, int sectionId = 9)
        => new(Guid.NewGuid(), 15, 65164, sectionId, requestorId);

    [Fact]
    public void StartReplacesTheRequestorsEarlierRunsOfTheSection()
    {
        var first = Service.Start(Run());
        var otherSection = Service.Start(Run(sectionId: 10));
        var otherUser = Service.Start(Run(requestorId: 401));

        var second = Service.Start(Run());

        Service.FindById(first.Id).Should().BeNull("the requestor's earlier run of the same section is replaced");
        Service.FindById(second.Id).Should().NotBeNull();
        Service.FindById(otherSection.Id).Should().NotBeNull();
        Service.FindById(otherUser.Id).Should().NotBeNull();
        second.Status.Should().Be(ReportAITestStatus.Pending);
        second.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FindPendingHonoursTheStatusAndAge()
    {
        var run = Service.Start(Run());

        Service.FindPending(400, 9, DateTime.UtcNow.AddMinutes(-30))!.Id.Should().Be(run.Id);
        Service.FindPending(400, 9, DateTime.UtcNow.AddMinutes(1)).Should().BeNull("a run older than the window is treated as lost");
        Service.FindPending(401, 9, DateTime.UtcNow.AddMinutes(-30)).Should().BeNull();

        Service.Complete(new ReportAITest(run.Id, 15, 65164, 9, 400) { Status = ReportAITestStatus.Completed, Output = "<p>Done</p>" });
        Service.FindPending(400, 9, DateTime.UtcNow.AddMinutes(-30)).Should().BeNull("a completed run is not pending");
    }

    [Fact]
    public void CompleteUpdatesTheStoredRunAndKeepsItsStart()
    {
        var run = Service.Start(Run());
        var started = run.CreatedOn;

        var stored = Service.Complete(new ReportAITest(run.Id, 15, 65164, 9, 400)
        {
            Status = ReportAITestStatus.Completed,
            Output = "<p>Done</p>",
            RequestCount = 12,
            PromptTokens = 120097,
            CompletionTokens = 6610,
            DurationMs = 60698,
            StoryCount = 26,
            Transcript = "[{\"stage\":\"Final\"}]",
        });

        stored.Id.Should().Be(run.Id);
        stored.CreatedOn.Should().Be(started);
        stored.CompletedOn.Should().NotBeNull();
        var found = Service.FindById(run.Id)!;
        found.Status.Should().Be(ReportAITestStatus.Completed);
        found.Output.Should().Be("<p>Done</p>");
        found.RequestCount.Should().Be(12);
        found.Transcript.Should().Be("[{\"stage\":\"Final\"}]");
        Context.ReportAITests.Count(t => t.Id == run.Id).Should().Be(1);
    }

    [Fact]
    public void CompleteNeverLeavesARunPendingAndAddsBackAForgottenOne()
    {
        var lost = Run();

        var stored = Service.Complete(new ReportAITest(lost.Id, 15, 65164, 9, 400) { Status = ReportAITestStatus.Pending, Error = "boom" });

        stored.Status.Should().Be(ReportAITestStatus.Failed, "a posted outcome that is still pending would be waited for forever");
        Service.FindById(lost.Id)!.Error.Should().Be("boom");
    }

    [Fact]
    public void DeleteRemovesOnlyTheRun()
    {
        var run = Service.Start(Run());
        var other = Service.Start(Run(requestorId: 401));

        Service.Delete(run.Id).Should().BeTrue();
        Service.Delete(run.Id).Should().BeFalse();
        Service.FindById(run.Id).Should().BeNull();
        Service.FindById(other.Id).Should().NotBeNull();
    }

    [Fact]
    public void PurgeRemovesRunsStartedBeforeTheCutoff()
    {
        var run = Service.Start(Run());
        Service.Start(Run(requestorId: 401));

        Service.Purge(DateTime.UtcNow.AddHours(-1)).Should().Be(0);
        Service.FindById(run.Id).Should().NotBeNull();

        Service.Purge(DateTime.UtcNow.AddMinutes(1)).Should().Be(2);
        Context.ReportAITests.Should().BeEmpty();
    }

    public void Dispose()
    {
        Context.EnsureDeleted();
        helper.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
