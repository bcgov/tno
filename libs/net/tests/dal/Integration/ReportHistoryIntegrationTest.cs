using System.Text.Json;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// Database integration tests for the report instance history that excludes and deduplicates
/// content in a new report instance. The history is a projection, so it must translate to SQL.
/// Run with 'TNO_TEST_POSTGRES' set (see PostgresFactAttribute).
/// </summary>
[Collection(PostgresCollection.Name)]
public class ReportHistoryIntegrationTest
{
    #region Variables
    private readonly PostgresFixture _fixture;
    #endregion

    #region Constructors
    public ReportHistoryIntegrationTest(PostgresFixture fixture)
    {
        _fixture = fixture;
    }
    #endregion

    #region Helpers
    private ReportService CreateService(TNOContext context)
        => new(context, _fixture.Principal, null!, Options.Create(new ElasticOptions()), null!, _fixture.Services, Options.Create(new JsonSerializerOptions()), TestLogger.For<ReportService>());

    private static Report AddReport(TNOContext context, int ownerId, string settings)
    {
        var templateId = context.ReportTemplates.AsNoTracking().Where(t => t.ReportType == ReportType.Content).OrderBy(t => t.Id).Select(t => t.Id).First();
        var report = new Report($"it-{Guid.NewGuid():N}", templateId, ownerId) { Settings = JsonDocument.Parse(settings) };
        context.Reports.Add(report);
        context.SaveChanges();
        return report;
    }

    private static ReportInstance AddInstance(TNOContext context, Report report, DateTime? sentOn, params Content[] content)
    {
        var instance = new ReportInstance(report.Id, report.OwnerId, content.Select((c, index) => new ReportInstanceContent(0, c.Id, "section", index)))
        {
            PublishedOn = sentOn ?? DateTime.UtcNow,
            SentOn = sentOn,
            Body = "<p>sent report output</p>",
        };
        context.ReportInstances.Add(instance);
        context.SaveChanges();
        return instance;
    }

    private static void Remove(TNOContext context, Report report, Folder? folder = null)
    {
        context.Reports.Where(r => r.Id == report.Id).ExecuteDelete();
        if (folder != null) context.Folders.Where(f => f.Id == folder.Id).ExecuteDelete();
    }
    #endregion

    #region Tests
    private sealed class Commands : DbCommandInterceptor
    {
        public List<string> Sql { get; } = [];
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Sql.Add(command.CommandText);
            return result;
        }
    }

    [PostgresFact]
    public void AnalysisHistoryProjectsNoSavedOutputAndTransfersEachStoryOnce()
    {
        var commands = new Commands();
        using var context = _fixture.CreateContext(commands);
        var owners = context.Users.AsNoTracking().OrderBy(u => u.Id).Select(u => u.Id).Take(2).ToArray();
        var report = AddReport(context, owners[0], "{}");
        try
        {
            context.ReportSections.AddRange(
                new ReportSection("section", ReportSectionType.Content, report.Id, null, null, null)
                { IsEnabled = true, Settings = JsonDocument.Parse("{\"label\":\"News\"}") },
                new ReportSection("headlines", ReportSectionType.Content, report.Id, null, null, null)
                { IsEnabled = false, SortOrder = 1, Settings = JsonDocument.Parse("{\"label\":\"Headlines\"}") });
            context.SaveChanges();
            var story = _fixture.AddContent(context, configure: c =>
            {
                c.Body = "Complete article text";
                c.Summary = "Article summary";
                c.Versions[owners[0]] = new() { Body = "Custom article text", Summary = "Custom summary" };
            });
            var old = AddInstance(context, report, DateTime.UtcNow.AddDays(-3), story);
            var previous = new ReportInstance(report.Id, owners[0],
                [new ReportInstanceContent(0, story.Id, "section"), new ReportInstanceContent(0, story.Id, "headlines", 4)])
            {
                PublishedOn = DateTime.UtcNow.AddDays(-2), SentOn = DateTime.UtcNow.AddDays(-2),
                Body = new string('x', 1_000_000), Subject = "Unused historical subject",
            };
            context.ReportInstances.Add(previous);
            context.SaveChanges();
            var otherOwner = new ReportInstance(report.Id, owners[1], [new ReportInstanceContent(0, story.Id, "section")])
            { PublishedOn = DateTime.UtcNow.AddDays(-1), SentOn = DateTime.UtcNow.AddDays(-1) };
            context.ReportInstances.Add(otherOwner);
            context.SaveChanges();
            AddInstance(context, report, null, story);
            var current = AddInstance(context, report, DateTime.UtcNow, story);
            AddInstance(context, report, DateTime.UtcNow, story);
            context.ChangeTracker.Clear();
            commands.Sql.Clear();

            var service = CreateService(context);
            var result = service.GetPreviousReportHistory(report.Id, current.Id, owners[0], 2);

            result.Instances.Select(i => i.Id).Should().Equal(previous.Id, old.Id);
            result.Content.Should().ContainSingle();
            result.Content[0].Body.Should().Be(story.Body);
            result.Content[0].Summary.Should().Be(story.Summary);
            result.Content[0].Versions[owners[0]].Body.Should().Be("Custom article text");
            result.Instances[0].Content.Should().HaveCount(2);
            result.Instances[0].Content.Single(c => c.SectionName == "headlines").SortOrder.Should().Be(4);
            result.Sections.Should().HaveCount(2);
            result.Sections.Single(s => s.Name == "headlines").IsEnabled.Should().BeFalse();
            context.ChangeTracker.Entries().Should().BeEmpty();
            var instanceQueries = commands.Sql.Where(sql => System.Text.RegularExpressions.Regex.IsMatch(sql, @"\breport_instance\b")).ToArray();
            instanceQueries.Should().NotBeEmpty();
            foreach (var sql in instanceQueries)
                sql.Should().NotMatchRegex(@"\b(body|subject|response)\b");
            var json = JsonSerializer.Serialize(result);
            json.Should().NotContain("Unused historical subject");
            json.Length.Should().BeLessThan(20_000);
            service.GetPreviousReportHistory(report.Id, current.Id, owners[0], 1).Instances.Select(i => i.Id).Should().Equal(previous.Id);
            service.GetPreviousReportHistory(report.Id, current.Id, owners[1], 2).Instances.Select(i => i.Id).Should().Equal(otherOwner.Id);
            service.GetPreviousReportHistory(report.Id, current.Id, owners[0], 0).Instances.Should().BeEmpty();
        }
        finally
        {
            Remove(context, report);
        }
    }

    [PostgresFact]
    public void ContentToExclude_IsTheCurrentAndPreviousSentContent()
    {
        using var context = _fixture.CreateContext();
        var ownerId = context.Users.AsNoTracking().OrderBy(u => u.Id).Select(u => u.Id).First();
        var report = AddReport(context, ownerId, "{}");
        try
        {
            var content = Enumerable.Range(0, 4).Select(_ => _fixture.AddContent(context)).ToArray();
            AddInstance(context, report, DateTime.UtcNow.AddDays(-3), content[0]);
            AddInstance(context, report, DateTime.UtcNow.AddDays(-2), content[1]);
            AddInstance(context, report, DateTime.UtcNow.AddDays(-1), content[2]);
            AddInstance(context, report, null, content[3]);
            context.ChangeTracker.Clear();

            var result = CreateService(context).GetReportInstanceContentToExclude(report.Id, ownerId);

            result.Should().BeEquivalentTo(new[] { content[1].Id, content[2].Id, content[3].Id });
        }
        finally
        {
            Remove(context, report);
        }
    }

    [PostgresFact]
    public async Task FolderSection_ExcludesHistoricalContentAndDuplicateTitles()
    {
        using var context = _fixture.CreateContext();
        var ownerId = context.Users.AsNoTracking().OrderBy(u => u.Id).Select(u => u.Id).First();
        var report = AddReport(context, ownerId, "{\"Content\":{\"ExcludeHistorical\":true,\"RemoveDuplicateTitles3Days\":true}}");
        var folder = new Folder(0, $"it-{Guid.NewGuid():N}", ownerId);
        context.Folders.Add(folder);
        context.SaveChanges();
        try
        {
            var sent = _fixture.AddContent(context, configure: c => c.Headline = "Already sent");
            var sentTitle = _fixture.AddContent(context, configure: c => c.Headline = $"Same headline {report.Id}");
            var duplicate = _fixture.AddContent(context, configure: c => c.Headline = $"same headline {report.Id} ");
            var fresh = _fixture.AddContent(context, configure: c => c.Headline = "New story");
            context.FolderContents.AddRange(new FolderContent(folder.Id, sent.Id, 0), new FolderContent(folder.Id, duplicate.Id, 1), new FolderContent(folder.Id, fresh.Id, 2));
            context.ReportSections.Add(new ReportSection("section", ReportSectionType.Content, report.Id, null, folder.Id, null));
            context.SaveChanges();
            AddInstance(context, report, DateTime.UtcNow.AddDays(-1), sent, sentTitle);
            var current = AddInstance(context, report, null);
            context.ChangeTracker.Clear();
            var loaded = context.Reports.AsNoTracking().Include(r => r.Sections).Single(r => r.Id == report.Id);

            var latest = await CreateService(context).FindContentWithElasticsearchAsync(loaded, null, ownerId);
            var byInstance = await CreateService(context).FindContentWithElasticsearchAsync(loaded, current.Id, ownerId);

            latest["section"].Hits.Hits.Select(h => h.Source.Id).Should().Equal(fresh.Id);
            byInstance["section"].Hits.Hits.Select(h => h.Source.Id).Should().Equal(fresh.Id);
        }
        finally
        {
            Remove(context, report, folder);
        }
    }
    #endregion
}
