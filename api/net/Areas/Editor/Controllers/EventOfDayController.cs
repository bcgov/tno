using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Folder;
using TNO.API.Helpers;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.DAL.Helpers;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Keycloak;
using TNO.TemplateEngine.Converters;
using TNO.TemplateEngine.Models.Reports;
using TemplateContent = TNO.TemplateEngine.Models.ContentModel;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>Edit historical Event of the Day source topics and save regenerated report output.</summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/event-of-the-day")]
[Route("api/[area]/reports/event-of-the-day")]
[Route("v{version:apiVersion}/[area]/reports/event-of-the-day")]
[Route("[area]/reports/event-of-the-day")]
[Produces(MediaTypeNames.Application.Json)]
public class EventOfDayController : ControllerBase
{
    private readonly IReportInstanceService _instances;
    private readonly IReportService _reportService;
    private readonly IFolderService _folders;
    private readonly ISettingService _settings;
    private readonly ITopicScoreService _scores;
    private readonly IContentService _content;
    private readonly IReportHelper _reports;
    private readonly ElasticOptions _elastic;
    private readonly JsonSerializerOptions _json;

    /// <summary>
    /// Creates a new instance of a EventOfDayController object, initializes with specified parameters.
    /// </summary>
    /// <param name="instances"></param>
    /// <param name="reportService"></param>
    /// <param name="folders"></param>
    /// <param name="settings"></param>
    /// <param name="scores"></param>
    /// <param name="content"></param>
    /// <param name="reports"></param>
    /// <param name="elastic"></param>
    /// <param name="json"></param>
    public EventOfDayController(IReportInstanceService instances, IReportService reportService, IFolderService folders,
        ISettingService settings, ITopicScoreService scores, IContentService content, IReportHelper reports,
        IOptions<ElasticOptions> elastic, IOptions<JsonSerializerOptions> json)
    {
        _instances = instances;
        _reportService = reportService;
        _folders = folders;
        _settings = settings;
        _scores = scores;
        _content = content;
        _reports = reports;
        _elastic = elastic.Value;
        _json = json.Value;
    }

    private int ReportId => int.TryParse(_settings.FindByName("EventOfTheDayReportId")?.Value, out var id)
        ? id : throw new ConfigurationException("Event of the Day report is not configured.");

    private int FolderId => int.TryParse(_settings.FindByName("EventOfTheDayFolderId")?.Value, out var id)
        ? id : throw new ConfigurationException("Event of the Day folder is not configured.");

    private Entities.ReportInstance GetInstance(long id)
    {
        var instance = _instances.FindById(id) ?? throw new NoContentException("Report instance does not exist.");
        if (instance.ReportId != ReportId) throw new NotAuthorizedException("This is not an Event of the Day report.");
        if (!instance.PublishedOn.HasValue) throw new BadRequestException("The report has no publication date.");
        return instance;
    }

    /// <summary>
    /// An unsaved instance for a past day that has no report. Its folder sections are collected by
    /// the folder's own filter, so the report entity carries each folder's filter.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private Entities.ReportInstance NewInstance(DateOnly date)
    {
        var reportId = ReportId;
        var (start, end) = EventOfDayReportQuery.GetDayRange(date);
        if (end > DateTime.UtcNow) throw new BadRequestException("Only a past date can be generated. Use the current folder for today's report.");
        if (_instances.FindInstancesInRange(reportId, start, end).Any())
            throw new BadRequestException("A report already exists for this date. Select it to regenerate it.");

        var report = _reportService.FindById(reportId, query => query
            .Include(r => r.Owner)
            .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
            .Include(r => r.Sections).ThenInclude(s => s.Filter)
            .Include(r => r.Sections).ThenInclude(s => s.Folder).ThenInclude(f => f!.Filter)
            .Include(r => r.Sections).ThenInclude(s => s.ChartTemplatesManyToMany).ThenInclude(c => c.ChartTemplate)
            .AsSplitQuery()) ?? throw new NoContentException("Event of the Day report does not exist.");
        var latest = _instances.FindInstancesForReportId(reportId, null, 0, 5).FirstOrDefault(i => i.PublishedOn.HasValue);
        return new Entities.ReportInstance(report.Id, report.OwnerId)
        {
            Report = report,
            PublishedOn = EventOfDayReportQuery.GetPublishedOn(date, latest?.PublishedOn),
        };
    }

    private static object Summary(Entities.ReportInstance instance, IEnumerable<long> contentIds) => new
    {
        instance.Id,
        instance.PublishedOn,
        instance.SentOn,
        instance.Subject,
        instance.Version,
        ContentIds = contentIds.Distinct().OrderBy(id => id).ToArray()
    };

    /// <summary>
    /// Whether a story belongs in the Event of the Day folder: it has a topic, which may be the system
    /// "Not Applicable" topic and may score zero, like the folder's collector requires. The topic charts
    /// and totals apply their own stricter rule.
    /// </summary>
    private static bool IsFolderContent(Entities.Content content) => content.TopicsManyToMany.Count != 0;

    /// <summary>
    /// The report instances published on the BC calendar 'date', and that day's stories with a topic
    /// or in one of those reports, with their calculated topic scores.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Find([FromQuery] DateOnly date)
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(date);
        var stories = _instances.FindTopicContentInRange(ReportId, start, end).ToArray();
        var scores = _scores.CalculateScores(stories.Select(c => c.Id));
        return new JsonResult(new
        {
            Instances = _instances.FindInstancesInRange(ReportId, start, end)
                .Select(i => Summary(i, i.ContentManyToMany.Select(c => c.ContentId))),
            Content = stories.Select(c => new FolderContentModel
            {
                ContentId = c.Id,
                Content = new ContentModel(c),
                CalculatedTopicScore = scores.TryGetValue(c.Id, out var score) ? score : 0
            })
        });
    }

    /// <summary>
    /// Add one of today's stories to the Event of the Day folder, so today's report includes it.
    /// The story must have a topic and be published today (BC), like the folder's collector requires.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("folder/{contentId:long}")]
    public IActionResult AddToFolder(long contentId)
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(EventOfDayReportQuery.GetDate(DateTime.UtcNow));
        var story = _instances.FindTopicContentInRange(ReportId, start, end)
            .FirstOrDefault(c => c.Id == contentId && IsFolderContent(c));
        if (story == null) return BadRequest("Only stories with a topic that were published today can be added to the folder.");
        _folders.AddContentToFolder(contentId, FolderId);
        return Ok();
    }

    /// <summary>Regenerate a saved instance's output without saving it.</summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/preview")]
    public Task<IActionResult> Preview(long id, [FromBody] EventOfDayPreviewRequest request)
        => PreviewAsync(GetInstance(id), request);

    /// <summary>Generate the output of a report for a past day that has none, without saving it.</summary>
    /// <param name="date"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("preview")]
    public Task<IActionResult> PreviewDate([FromQuery] DateOnly date, [FromBody] EventOfDayPreviewRequest request)
        => PreviewAsync(NewInstance(date), request);

    private async Task<IActionResult> PreviewAsync(Entities.ReportInstance instance, EventOfDayPreviewRequest request)
    {
        var model = new Services.Models.ReportInstance.ReportInstanceModel(instance, _json);
        var report = model.Report!;
        var publishedOn = instance.PublishedOn!.Value;
        var sections = new Dictionary<string, ReportSectionModel>();
        var links = new List<EventOfDayContentLink>();
        var (start, end) = EventOfDayReportQuery.GetDayRange(EventOfDayReportQuery.GetDate(publishedOn));
        var dayStories = _instances.FindTopicContentInRange(instance.ReportId, start, end).ToArray();
        var changed = dayStories.Where(c => request.ChangedContentIds.Contains(c.Id)).ToArray();
        // Stories an editor adds by hand must have a topic and be published on the report's day.
        var added = request.AddContentIds.Distinct()
            .Select(id => dayStories.FirstOrDefault(c => c.Id == id && IsFolderContent(c)))
            .ToArray();
        if (added.Any(c => c == null))
            return BadRequest("Only stories with a topic that were published on this report's date can be added.");
        var checkedIndexes = new HashSet<string>();
        Entities.ReportInstanceContent[]? savedContent = null;

        foreach (var section in report.Sections.OrderBy(s => s.SortOrder))
        {
            var content = new List<TemplateContent>();
            Dictionary<string, AggregationRootModel>? aggregations = null;
            var entitySection = instance.Report!.Sections.FirstOrDefault(s => s.Id == section.Id);
            // A generated day has no saved folder, and a re-collected one ignores its saved links:
            // either way the folder sections are collected by the folder's own filter.
            var folderFilter = (instance.Id == 0 || request.Recollect) && section.IsEnabled && section.Filter == null
                ? entitySection?.Folder?.Filter
                : null;
            if (section.IsEnabled && section.Filter != null)
            {
                var index = section.Filter.Settings.SearchUnpublished ? _elastic.ContentIndex : _elastic.PublishedIndex;
                if (checkedIndexes.Add(index) && !await TopicsAreIndexed(index, changed))
                    return Conflict("Topic changes are still being indexed. Wait a few seconds, then regenerate again.");

                // Retain the configured aggregations (including top-five limits), and anchor
                // relative dates to the historical instance instead of today's date.
                var (hits, aggs) = await SearchAsync(index, section.Filter.Query, publishedOn);
                aggregations = aggs;
                foreach (var hit in hits)
                {
                    content.Add(new TemplateContent(hit, content.Count));
                    links.Add(new EventOfDayContentLink { ContentId = hit.Id, SectionName = section.Name, SortOrder = content.Count - 1 });
                }
            }
            else if (folderFilter != null)
            {
                var filterSettings = JsonSerializer.Deserialize<FilterSettingsModel>(folderFilter.Settings, _json) ?? new();
                var index = filterSettings.SearchUnpublished ? _elastic.ContentIndex : _elastic.PublishedIndex;
                if (checkedIndexes.Add(index) && !await TopicsAreIndexed(index, changed))
                    return Conflict("Topic changes are still being indexed. Wait a few seconds, then regenerate again.");

                // The collector sorts newest first; keep that order so the sections read like a collected folder.
                var (hits, _) = await SearchAsync(index, folderFilter.Query, publishedOn);
                foreach (var hit in hits.OrderByDescending(h => h.PublishedOn).ThenBy(h => h.Id))
                {
                    content.Add(new TemplateContent(hit, content.Count));
                    links.Add(new EventOfDayContentLink { ContentId = hit.Id, SectionName = section.Name, SortOrder = content.Count - 1 });
                }
                AppendAdded(section.Name, added!, content, links);
            }
            else if (instance.Id != 0)
            {
                // A historical folder has already been cleared. Use its saved associations;
                // never read today's live folder to rebuild an earlier report.
                savedContent ??= _instances.GetContentForInstance(instance.Id).ToArray();
                foreach (var link in savedContent.Where(c => c.SectionName == section.Name && c.Content != null))
                {
                    content.Add(new TemplateContent(link.Content!, link.SortOrder));
                    links.Add(new EventOfDayContentLink { ContentId = link.ContentId, SectionName = section.Name, SortOrder = link.SortOrder });
                }
                // Added stories join every section the folder feeds, as the collector would have placed them.
                if (entitySection?.FolderId.HasValue == true) AppendAdded(section.Name, added!, content, links);
            }
            sections.Add(section.Name, new ReportSectionModel(section, content, aggregations));
        }
        var output = await _reports.GenerateReportSectionsAsync(model, sections);
        return new JsonResult(new EventOfDayPreview
        {
            InstanceId = instance.Id,
            ReportId = instance.ReportId,
            Version = instance.Version,
            PublishedOn = publishedOn,
            Subject = output.Subject,
            Body = output.Body,
            Content = links.ToArray()
        });
    }

    /// <summary>Append hand-picked stories that the section does not already hold, after its collected content.</summary>
    private static void AppendAdded(string sectionName, Entities.Content[] added, List<TemplateContent> content, List<EventOfDayContentLink> links)
    {
        var next = links.Where(l => l.SectionName == sectionName).Select(l => l.SortOrder + 1).DefaultIfEmpty(0).Max();
        foreach (var story in added.Where(s => !links.Any(l => l.SectionName == sectionName && l.ContentId == s.Id)))
        {
            content.Add(new TemplateContent(story, next));
            links.Add(new EventOfDayContentLink { ContentId = story.Id, SectionName = sectionName, SortOrder = next++ });
        }
    }

    /// <summary>Run a report filter as it would have run on 'publishedOn', fetching every hit.</summary>
    private async Task<(List<Services.Models.Content.ContentModel> Hits, Dictionary<string, AggregationRootModel>? Aggregations)> SearchAsync(string index, JsonDocument filter, DateTime publishedOn)
    {
        var hits = new List<Services.Models.Content.ContentModel>();
        Dictionary<string, AggregationRootModel>? aggregations = null;
        var query = EventOfDayReportQuery.Create(filter, publishedOn);
        while (true)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var result = await _content.FindWithElasticsearchAsync(index, JsonSerializer.SerializeToDocument(query));
            if (result.TimeOut || result.Shards?.Failed > 0)
                throw new InvalidOperationException("The report search was incomplete. Please regenerate again.");
            aggregations ??= result.Aggregations?.ToDictionary(a => a.Key, a => a.Value.Convert());
            var page = result.Hits.Hits.ToArray();
            hits.AddRange(page.Select(h => h.Source));
            if (page.Length < 1000) break;
            // Fetch all chart content, rather than silently truncating it at 1,024/10,000.
            query["search_after"] = new JsonArray(JsonValue.Create(page[^1].Source.Id));
            query.Remove("aggs");
            query.Remove("aggregations");
        }
        return (hits, aggregations);
    }

    private async Task<bool> TopicsAreIndexed(string index, Entities.Content[] changed)
    {
        foreach (var batch in changed.Where(c => index == _elastic.ContentIndex || c.Status == Entities.ContentStatus.Published).Chunk(500))
        {
            var query = JsonSerializer.SerializeToDocument(new
            {
                size = batch.Length,
                query = new { terms = new { id = batch.Select(c => c.Id) } }
            });
            var result = await _content.FindWithElasticsearchAsync(index, query);
            if (result.TimeOut || result.Shards?.Failed > 0) return false;
            var indexed = result.Hits.Hits.ToDictionary(h => h.Source.Id, h => h.Source);
            foreach (var story in batch)
            {
                if (!indexed.TryGetValue(story.Id, out var hit)) return false;
                // The indexed document may not carry the system flag; the ID and score identify the assignment.
                var expected = story.TopicsManyToMany.OrderBy(t => t.TopicId).Select(t => (t.TopicId, t.Score));
                var actual = hit.Topics.OrderBy(t => t.Id).Select(t => (t.Id, t.Score));
                if (!expected.SequenceEqual(actual)) return false;
            }
        }
        return true;
    }

    /// <summary>Overwrite a saved instance's output with a reviewed preview.</summary>
    /// <param name="id"></param>
    /// <param name="preview"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/output")]
    public IActionResult Save(long id, [FromBody] EventOfDayPreview preview)
    {
        var instance = GetInstance(id);
        if (preview.InstanceId != id) return BadRequest("Regenerate and preview this report before saving.");
        var error = Validate(preview, instance);
        if (error != null) return BadRequest(error);
        var saved = _instances.SaveReportOutput(instance, preview.Version, preview.Subject, preview.Body, ToLinks(preview, id));
        return new JsonResult(Summary(saved, preview.Content.Select(c => c.ContentId)));
    }

    /// <summary>Save a generated preview as the report instance for a past day that has none.</summary>
    /// <param name="date"></param>
    /// <param name="preview"></param>
    /// <returns></returns>
    [HttpPost("output")]
    public IActionResult SaveDate([FromQuery] DateOnly date, [FromBody] EventOfDayPreview preview)
    {
        var template = NewInstance(date);
        if ((preview.InstanceId ?? 0) != 0 || !preview.PublishedOn.HasValue) return BadRequest("Generate and preview this report before saving.");
        var error = Validate(preview, template);
        if (error != null) return BadRequest(error);
        var (start, end) = EventOfDayReportQuery.GetDayRange(date);
        // The report navigation stays off the new row so EF never tries to insert the report itself.
        var instance = new Entities.ReportInstance(template.ReportId, template.OwnerId)
        {
            PublishedOn = DateTime.SpecifyKind(preview.PublishedOn.Value, DateTimeKind.Utc),
            Subject = preview.Subject,
            Body = preview.Body,
        };
        var saved = _instances.AddReportOutput(instance, start, end, ToLinks(preview, 0));
        return new JsonResult(Summary(saved, preview.Content.Select(c => c.ContentId)));
    }

    private static string? Validate(EventOfDayPreview preview, Entities.ReportInstance instance)
    {
        if (preview.ReportId != instance.ReportId || string.IsNullOrWhiteSpace(preview.Body))
            return "Regenerate and preview this report before saving.";
        var sectionNames = instance.Report!.Sections.Select(s => s.Name).ToHashSet();
        if (preview.Content.Any(c => c.ContentId <= 0 || !sectionNames.Contains(c.SectionName)) ||
            preview.Content.Select(c => (c.ContentId, c.SectionName)).Distinct().Count() != preview.Content.Length)
            return "The preview contains invalid report content.";
        return null;
    }

    private static IEnumerable<Entities.ReportInstanceContent> ToLinks(EventOfDayPreview preview, long instanceId)
        => preview.Content.Select(c => new Entities.ReportInstanceContent(instanceId, c.ContentId, c.SectionName, c.SortOrder));
}

/// <summary>What to change when a report is generated or regenerated for preview.</summary>
public class EventOfDayPreviewRequest
{
    /// <summary>Stories whose topics were edited; the preview waits until they are re-indexed.</summary>
    public long[] ChangedContentIds { get; set; } = Array.Empty<long>();
    /// <summary>Stories to add to the folder sections; each must have a topic and be published on the report's day.</summary>
    public long[] AddContentIds { get; set; } = Array.Empty<long>();
    /// <summary>Rebuild the folder sections from the folder's collector filter instead of the saved links.</summary>
    public bool Recollect { get; set; }
}

/// <summary>A story's place in one section of a report instance.</summary>
public class EventOfDayContentLink
{
    /// <summary>The story's content ID.</summary>
    public long ContentId { get; set; }
    /// <summary>The report section's name.</summary>
    public string SectionName { get; set; } = "";
    /// <summary>The story's position within the section.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Generated report output with the links that produced it, returned by a preview and sent back to save it.
/// </summary>
public class EventOfDayPreview : ReportResultModel
{
    /// <summary>The instance version the preview was generated from; saving fails if it changed.</summary>
    public long Version { get; set; }
    /// <summary>The instance's publication date, or the one a generated day would be saved with.</summary>
    public DateTime? PublishedOn { get; set; }
    /// <summary>The stories in each section of the generated output.</summary>
    public EventOfDayContentLink[] Content { get; set; } = Array.Empty<EventOfDayContentLink>();
}
