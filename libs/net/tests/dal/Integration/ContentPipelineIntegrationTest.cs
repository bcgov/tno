using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.DAL;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// Database integration tests for the raw SQL paths: index requests, analysis requests, result
/// submission and the analysis runs kept in content metadata, report AI result claims, and history
/// purges.
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
        => new(context, _fixture.Principal, _fixture.Services, TestLogger.For<ContentAnalysisService>());

    private TopicScoreService CreateTopicScoreService(TNOContext context)
        => new(context, _fixture.Principal, _fixture.Services, Options.Create(new TopicScoreOptions()), TestLogger.For<TopicScoreService>());

    private static AnalysisRun Run(string requestId, AnalysisRunStatus status, DateTime requestedOn, long? workOrderId = null)
        => new() { RequestId = requestId, Status = status, RequestedOn = requestedOn, FinishedOn = DateTime.UtcNow, Attempts = 1, InputHash = "hash", WorkOrderId = workOrderId };
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

    #region Analysis
    [PostgresFact]
    public void Analysis_NewContentRecordsALifecycleRequest()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);

        var request = context.TakeAnalysisRequests().Single(r => r.ContentId == content.Id);
        request.Reason.Should().Be(AnalysisRequestReason.Lifecycle);
        request.Force.Should().BeFalse();
        request.InputHash.Should().NotBeNullOrWhiteSpace();
        context.TakeAnalysisRequests().Should().BeEmpty("requests are taken once");
    }

    [PostgresFact]
    public void Analysis_OnlyAnalysisInputChangesRecordARequest()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        context.TakeAnalysisRequests();

        content.Page = "A2";
        context.SaveChanges();
        context.TakeAnalysisRequests().Should().BeEmpty("the page is not an analysis input");

        content.Headline = "A changed headline";
        context.SaveChanges();
        context.TakeAnalysisRequests().Should().ContainSingle(r => r.ContentId == content.Id && r.Reason == AnalysisRequestReason.Lifecycle);
    }

    [PostgresFact]
    public void Analysis_RolledBackSaveSendsNoRequest()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        context.TakeAnalysisRequests();

        using (var transaction = context.Database.BeginTransaction())
        {
            content.Headline = "A change that is rolled back";
            context.SaveChanges();
            transaction.Rollback();
        }
        context.TakeAnalysisRequests().Should().BeEmpty();
    }

    [PostgresFact]
    public void Analysis_SubmitAppliesOnlyTheProcessesRunAndRecordsTheRun()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context, ContentStatus.Draft, c => c.Summary = "");
        var input = CreateAnalysisService(context).GetInput(content.Id)!;
        input.AnalysisInputHash.Should().BeNull();
        var request = new AnalysisRequestRunModel() { RequestId = Guid.NewGuid().ToString("N"), Reason = AnalysisRequestReason.Lifecycle, InputHash = input.InputHash, RequestedOn = DateTime.UtcNow, Attempts = 1 };
        var result = new AnalysisResultModel()
        {
            ContentId = content.Id,
            Request = request,
            InputHash = input.InputHash,
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

        // An analysis of an input the content no longer has is rejected.
        using var staleContext = _fixture.CreateContext();
        CreateAnalysisService(staleContext).Submit(new AnalysisResultModel() { ContentId = content.Id, Request = request, InputHash = "out-of-date" })
            .Status.Should().Be(ContentAnalysisService.Stale);

        using var submitting = _fixture.CreateContext();
        var accepted = CreateAnalysisService(submitting).Submit(result);
        accepted.Status.Should().Be(ContentAnalysisService.Accepted);
        accepted.PopulatedFields.Should().BeEquivalentTo(new[] { "summary", "quotes" }, "only the processes the worker ran are applied");
        submitting.TakeAnalysisRequests().Should().BeEmpty("analysis's own changes are not sent back to it");

        using var verify = _fixture.CreateContext();
        verify.Contents.AsNoTracking().Single(c => c.Id == content.Id).Summary.Should().Be(result.Summary);
        verify.ContentAnalyses.AsNoTracking().Where(a => a.ContentId == content.Id && a.IsCurrent).Should().ContainSingle();
        TakeRequests(submitting, content.Id).Should().ContainSingle(o => o.Reason == TNOContext.IndexReasonAnalysis);
        var runs = CreateAnalysisService(verify).FindRuns(content.Id);
        runs.Status.Should().Be(AnalysisRunStatus.Completed);
        runs.Runs.Should().ContainSingle(r => r.RequestId == request.RequestId && r.AnalysisId == accepted.AnalysisId);
        CreateAnalysisService(verify).GetInput(content.Id)!.AnalysisInputHash.Should().Be(input.InputHash);

        // Resubmitting the same input is a duplicate, not a second analysis.
        using var duplicate = _fixture.CreateContext();
        CreateAnalysisService(duplicate).Submit(result).Status.Should().Be(ContentAnalysisService.Duplicate);
        verify.ContentAnalyses.AsNoTracking().Count(a => a.ContentId == content.Id).Should().Be(1);
    }

    [PostgresFact]
    public void Analysis_RunsKeepTheNewestRequestAsTheStatus()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        var version = context.Contents.AsNoTracking().Where(c => c.Id == content.Id).Select(c => c.Version).First();
        var service = CreateAnalysisService(context);
        var now = DateTime.UtcNow;

        service.RecordRun(content.Id, Run("newer", AnalysisRunStatus.Completed, now));
        // An older request that finishes later does not replace the newer outcome.
        service.RecordRun(content.Id, Run("older", AnalysisRunStatus.Failed, now.AddMinutes(-5)));
        // A retry of a request updates its run.
        service.RecordRun(content.Id, Run("newer", AnalysisRunStatus.Completed, now));

        using var verify = _fixture.CreateContext();
        var runs = CreateAnalysisService(verify).FindRuns(content.Id);
        runs.Status.Should().Be(AnalysisRunStatus.Completed);
        runs.Runs.Select(r => r.RequestId).Should().Equal("newer", "older");
        verify.Contents.AsNoTracking().Where(c => c.Id == content.Id).Select(c => c.Version).First().Should().Be(version, "recording a run does not change the content's version");

        for (var i = 0; i < AnalysisMetadata.MaxRuns + 2; i++)
            service.RecordRun(content.Id, Run($"run-{i}", AnalysisRunStatus.Completed, now.AddMinutes(i + 1)));
        CreateAnalysisService(verify).FindRuns(content.Id).Runs.Should().HaveCount(AnalysisMetadata.MaxRuns);
    }

    [PostgresFact]
    public void Analysis_SavingContentDoesNotOverwriteItsMetadata()
    {
        using var context = _fixture.CreateContext();
        var content = _fixture.AddContent(context);
        using (var recording = _fixture.CreateContext())
            CreateAnalysisService(recording).RecordRun(content.Id, Run("kept", AnalysisRunStatus.Skipped, DateTime.UtcNow));

        // The tracked entity still holds the metadata it was created with.
        content.Headline = "A change saved from a stale entity";
        context.SaveChanges();

        using var verify = _fixture.CreateContext();
        CreateAnalysisService(verify).FindRuns(content.Id).Runs.Should().ContainSingle(r => r.RequestId == "kept");
    }

    [PostgresFact]
    public void Analysis_FailuresListTheContentWhoseNewestRequestFailed()
    {
        using var context = _fixture.CreateContext();
        var failed = _fixture.AddContent(context);
        var recovered = _fixture.AddContent(context);
        var service = CreateAnalysisService(context);
        var workOrderId = -Random.Shared.NextInt64(1, long.MaxValue);
        var now = DateTime.UtcNow;

        service.RecordRun(failed.Id, Run("failed", AnalysisRunStatus.Failed, now, workOrderId));
        service.RecordRun(recovered.Id, Run("failed", AnalysisRunStatus.Failed, now, workOrderId));
        service.RecordRun(recovered.Id, Run("recovered", AnalysisRunStatus.Completed, now.AddMinutes(1)));

        service.FindFailures(1000).Select(f => f.ContentId).Should().Contain(failed.Id).And.NotContain(recovered.Id);
        service.CountFailures(workOrderId).Should().Be(1);
    }
    #endregion

    #region Topic rescore
    [PostgresFact]
    public void TopicRescore_IsAWorkOrderRescoredAPageAtATime()
    {
        using var context = _fixture.CreateContext();
        var topic = context.Topics.AsNoTracking().OrderBy(t => t.Id).FirstOrDefault();
        if (topic == null) return; // No topic to score.

        // A minute no other content is published in.
        var publishedOn = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(Random.Shared.Next(1, 500_000));
        var first = _fixture.AddContent(context, ContentStatus.Draft, c => c.PublishedOn = publishedOn);
        var second = _fixture.AddContent(context, ContentStatus.Draft, c => c.PublishedOn = publishedOn);
        context.ContentTopics.Add(new ContentTopic(first.Id, topic.Id, 0));
        context.ContentTopics.Add(new ContentTopic(second.Id, topic.Id, 0));
        context.SaveChanges();

        var service = CreateTopicScoreService(context);
        var workOrder = service.AddRescore(publishedOn, publishedOn.AddMinutes(1), null, null);
        try
        {
            workOrder.WorkType.Should().Be(WorkOrderType.TopicRescore);
            workOrder.Status.Should().Be(WorkOrderStatus.Submitted);
            var configuration = TopicScoreService.ReadRescoreConfiguration(service.FindRescore(workOrder.Id)!);
            configuration.Total.Should().Be(2);
            service.FindRescores().Should().Contain(w => w.Id == workOrder.Id);

            var page = service.RescorePage(configuration, 0, 1);
            page.Processed.Should().Be(1);
            page.LastContentId.Should().Be(Math.Min(first.Id, second.Id));
            page.IsLast.Should().BeFalse();

            page = service.RescorePage(configuration, page.LastContentId, 1);
            page.LastContentId.Should().Be(Math.Max(first.Id, second.Id));

            page = service.RescorePage(configuration, page.LastContentId, 1);
            page.Processed.Should().Be(0);
            page.IsLast.Should().BeTrue();
        }
        finally
        {
            context.Database.ExecuteSqlRaw("DELETE FROM public.work_order WHERE id = {0}", workOrder.Id);
        }
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
