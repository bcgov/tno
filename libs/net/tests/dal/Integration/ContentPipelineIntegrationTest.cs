using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.DAL;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// Database integration tests for the raw SQL paths: index requests, analysis job claims and
/// result submission, report AI result claims, and history purges.
/// Run with 'TNO_TEST_POSTGRES' set (see PostgresFactAttribute).
/// </summary>
[Collection(PostgresCollection.Name)]
public class ContentPipelineIntegrationTest
{
    #region Variables
    private readonly PostgresFixture _fixture;
    #endregion

    #region Constructors
    public ContentPipelineIntegrationTest(PostgresFixture fixture)
    {
        _fixture = fixture;
    }
    #endregion

    #region Helpers
    private static SavedIndexRequest[] TakeRequests(TNOContext context, long contentId)
        => context.TakeIndexRequests().Where(r => r.ContentId == contentId).ToArray();

    private static long GetRevision(TNOContext context, long contentId)
        => context.Contents.AsNoTracking().Where(c => c.Id == contentId).Select(c => c.ProjectionRevision).First();

    private ContentAnalysisService CreateAnalysisService(TNOContext context)
        => new(context, _fixture.Principal, _fixture.Services, Options.Create(new ContentAnalysisOptions()), TestLogger.For<ContentAnalysisService>());

    /// <summary>
    /// Make the content's job due now, ahead of every other job.
    /// </summary>
    private static void MakeJobDue(TNOContext context, long contentId)
    {
        // The newest test content always has the highest priority.
        context.Database.ExecuteSqlRaw("UPDATE public.analysis_job SET due_on = CURRENT_TIMESTAMP - interval '1 second', priority = {1} WHERE content_id = {0}", contentId, (int)(1_000_000 + contentId % 1_000_000_000));
    }
    #endregion

    #region Index requests
    [PostgresFact]
    public void IndexRequest_IsRecordedWithTheContent()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context, ContentStatus.Publish, c => context.RequestIndex(c, IndexRequestAction.Publish));

        var requests = TakeRequests(context, content.Id);
        requests.Should().ContainSingle();
        requests[0].Action.Should().Be(IndexRequestAction.Publish);
        requests[0].ProjectionRevision.Should().Be(1);
        requests[0].Reason.Should().Be(TNOContext.IndexReasonLifecycle);
        content.ProjectionRevision.Should().Be(1);
        GetRevision(context, content.Id).Should().Be(1);
        context.TakeIndexRequests().Should().BeEmpty("requests are taken once");
    }

    [PostgresFact]
    public void IndexRequest_DerivesTheActionFromTheSavedStatus()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);

        context.RequestIndex(content.Id);
        content.Status = ContentStatus.Unpublish;
        context.SaveChanges();

        var request = TakeRequests(context, content.Id).Single();
        request.Action.Should().Be(IndexRequestAction.Unpublish);
        request.ProjectionRevision.Should().Be(1);
    }

    [PostgresFact]
    public void IndexRequest_RevisionIncrementsWithoutChangingTheVersion()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        var version = context.Contents.AsNoTracking().Where(c => c.Id == content.Id).Select(c => c.Version).First();

        context.RequestIndex(content.Id, IndexRequestAction.Index);
        context.RequestIndex(content.Id, IndexRequestAction.Index);
        context.SaveChanges();

        TakeRequests(context, content.Id).Select(o => o.ProjectionRevision).Should().Equal(1, 2);
        context.Contents.AsNoTracking().Where(c => c.Id == content.Id).Select(c => c.Version).First().Should().Be(version);
    }

    [PostgresFact]
    public void IndexRequest_DeleteCarriesTheNextRevision()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context, ContentStatus.Draft, c => context.RequestIndex(c, IndexRequestAction.Index));

        using var deleting = _fixture.CreateContext();
        deleting.RequestIndex(content.Id, IndexRequestAction.Delete);
        deleting.Contents.Remove(deleting.Contents.First(c => c.Id == content.Id));
        deleting.SaveChanges();

        var request = TakeRequests(deleting, content.Id).Single();
        request.Action.Should().Be(IndexRequestAction.Delete);
        request.ProjectionRevision.Should().Be(2);
    }

    [PostgresFact]
    public void IndexRequest_RolledBackChangeSendsNothing()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        context.TakeIndexRequests();

        using (var transaction = context.Database.BeginTransaction())
        {
            context.RequestIndex(content.Id, IndexRequestAction.Index);
            content.Headline = "Changed then rolled back";
            context.SaveChanges();
            transaction.Rollback();
        }

        TakeRequests(context, content.Id).Should().BeEmpty();
        GetRevision(context, content.Id).Should().Be(0);
    }

    [PostgresFact]
    public void IndexRequest_FailedSaveSendsNothing()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        context.TakeIndexRequests();

        // A stale version fails the save (optimistic concurrency).
        context.RequestIndex(content.Id, IndexRequestAction.Index);
        content.Headline = "Stale change";
        context.Entry(content).Property(c => c.Version).OriginalValue = content.Version - 1;
        var act = () => context.CommitTransaction();

        act.Should().Throw<DbUpdateConcurrencyException>();
        TakeRequests(context, content.Id).Should().BeEmpty();
    }
    #endregion

    #region Analysis jobs
    [PostgresFact]
    public void Analysis_NewContentSchedulesAJobAfterTheQuietPeriod()
    {
        using var context = _fixture.CreateContext();
        var before = DateTime.UtcNow;
        var content = _fixture.AddContent(context);

        var job = context.AnalysisJobs.AsNoTracking().Single(j => j.ContentId == content.Id);
        job.Status.Should().Be(AnalysisJobStatus.Pending);
        job.Reason.Should().Be(AnalysisJobReason.Lifecycle);
        job.DueOn.Should().BeOnOrAfter(before.AddSeconds(new ContentAnalysisOptions().QuietPeriodSeconds - 1));
    }

    [PostgresFact]
    public void Analysis_AJobIsClaimedOnce()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        MakeJobDue(context, content.Id);

        using var first = _fixture.CreateContext();
        using var second = _fixture.CreateContext();
        var request = new AnalysisClaimRequestModel() { WorkerId = "it", Quantity = 1, MaxBackfill = 0 };
        var claimed = CreateAnalysisService(first).ClaimJobs(request).ToArray();
        var again = CreateAnalysisService(second).ClaimJobs(request).ToArray();

        claimed.Should().ContainSingle(j => j.ContentId == content.Id);
        claimed[0].Status.Should().Be(AnalysisJobStatus.Claimed);
        claimed[0].FencingToken.Should().Be(1);
        again.Should().NotContain(j => j.ContentId == content.Id);
    }

    [PostgresFact]
    public void Analysis_SubmitAppliesOnlyTheProcessesRunAndRejectsStaleClaims()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context, ContentStatus.Draft, c => c.Summary = "");
        MakeJobDue(context, content.Id);

        var job = CreateAnalysisService(context).ClaimJobs(new AnalysisClaimRequestModel() { WorkerId = "it", Quantity = 1, MaxBackfill = 0 }).Single();
        job.ContentId.Should().Be(content.Id);
        var result = new AnalysisResultModel()
        {
            JobId = job.Id,
            FencingToken = job.FencingToken,
            InputHash = job.InputHash,
            Processes = AnalysisProcess.Summary | AnalysisProcess.Quotes,
            NormalizationVersion = "1",
            SchemaVersion = "1",
            PromptVersion = "1",
            Model = "fake",
            Summary = "The minister said the hospital will open in the spring.",
            PrimaryTopic = "Health care",
            SuggestedContributor = "Jane Doe",
            Quotes = new[] { new AnalysisQuoteModel() { Statement = "the hospital will open in the spring", Speaker = "The minister" } },
        };

        // A stale fencing token (a lapsed claim) is rejected.
        using var staleContext = _fixture.CreateContext();
        CreateAnalysisService(staleContext).Submit(new AnalysisResultModel() { JobId = job.Id, FencingToken = job.FencingToken - 1, InputHash = job.InputHash })
            .Status.Should().Be(ContentAnalysisService.Stale);

        using var submitting = _fixture.CreateContext();
        var accepted = CreateAnalysisService(submitting).Submit(result);
        accepted.Status.Should().Be(ContentAnalysisService.Accepted);
        accepted.PopulatedFields.Should().BeEquivalentTo(new[] { "summary", "quotes" }, "only the processes the worker ran are applied");

        using var verify = _fixture.CreateContext();
        verify.Contents.AsNoTracking().Single(c => c.Id == content.Id).Summary.Should().Be(result.Summary);
        verify.ContentAnalyses.AsNoTracking().Where(a => a.ContentId == content.Id && a.IsCurrent).Should().ContainSingle();
        verify.AnalysisJobs.AsNoTracking().Single(j => j.Id == job.Id).Status.Should().Be(AnalysisJobStatus.Completed);
        TakeRequests(submitting, content.Id).Should().ContainSingle(o => o.Reason == TNOContext.IndexReasonAnalysis);

        // Resubmitting the same input is a duplicate, not a second analysis.
        using var duplicate = _fixture.CreateContext();
        CreateAnalysisService(duplicate).Submit(result).Status.Should().Be(ContentAnalysisService.Duplicate);
        verify.ContentAnalyses.AsNoTracking().Count(a => a.ContentId == content.Id).Should().Be(1);
    }
    #endregion

    #region Report AI results
    [PostgresFact]
    public void ReportAIResult_OnlyOneGeneratorHoldsTheClaim()
    {
        using var context = _fixture.CreateContext();
        var section = context.ReportSections.AsNoTracking().OrderBy(s => s.Id).FirstOrDefault();
        if (section == null) return; // No report to attach a result to.

        var hash = Guid.NewGuid().ToString("N");
        var claim = new ReportAIResultClaimModel() { Hash = hash, ReportId = section.ReportId, ReportSectionId = section.Id, PipelineVersion = "it", LeaseSeconds = 60 };
        try
        {
            using var first = _fixture.CreateContext();
            using var second = _fixture.CreateContext();
            new ReportAIResultService(first, _fixture.Principal, _fixture.Services, TestLogger.For<ReportAIResultService>()).TryClaim(claim, "it-1").Should().NotBeNull();
            new ReportAIResultService(second, _fixture.Principal, _fixture.Services, TestLogger.For<ReportAIResultService>()).TryClaim(claim, "it-2").Should().BeNull();

            // A failed result can be claimed again.
            context.Database.ExecuteSqlRaw("UPDATE public.report_ai_result SET status = 2 WHERE hash = {0}", hash);
            using var third = _fixture.CreateContext();
            new ReportAIResultService(third, _fixture.Principal, _fixture.Services, TestLogger.For<ReportAIResultService>()).TryClaim(claim, "it-3").Should().NotBeNull();
        }
        finally
        {
            context.Database.ExecuteSqlRaw("DELETE FROM public.report_ai_result WHERE hash = {0}", hash);
        }
    }
    #endregion

    #region History retention
    [PostgresFact]
    public void Retention_DryRunsExecute()
    {
        using var context = _fixture.CreateContext();
        var service = new HistoryRetentionService(context, _fixture.Principal, _fixture.Services, Options.Create(new System.Text.Json.JsonSerializerOptions()), TestLogger.For<HistoryRetentionService>());

        var reports = service.PurgeReports(90, dryRun: true);
        var notifications = service.PurgeNotifications(30, 7, dryRun: true);

        reports.Should().NotBeNull();
        notifications.Should().NotBeNull();
    }
    #endregion
}
