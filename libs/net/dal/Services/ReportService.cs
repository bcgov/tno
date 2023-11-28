using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Variables
    private readonly ElasticOptions _elasticOptions;
    private readonly ITNOElasticClient _elasticClient;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    public ReportService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IServiceProvider serviceProvider,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the reports.
    /// </summary>
    /// <param name="populateFullModel"></param>
    /// <returns></returns>
    public IEnumerable<Report> FindAll(bool populateFullModel = true)
    {
        var query = this.Context.Reports
            .AsNoTracking()
            .Include(f => f.Owner)
            .AsQueryable();

        if (populateFullModel)
            query = query
                .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
                .Include(r => r.Sections)
                .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
                .Include(r => r.Events).ThenInclude(s => s.Schedule)
                .AsQueryable();

        return query
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
            .ToArray();
    }

    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="populateFullModel"></param>
    /// <returns></returns>
    public IEnumerable<Report> Find(ReportFilter filter, bool populateFullModel = true)
    {
        var query = this.Context.Reports
            .AsNoTracking()
            .Include(f => f.Owner)
            .AsQueryable();

        if (populateFullModel)
            query = query
                .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
                .Include(r => r.Sections)
                .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
                .Include(r => r.Events).ThenInclude(s => s.Schedule)
                .AsQueryable();

        if (filter.IsPublic.HasValue)
            query = query.Where(r => r.IsPublic);

        if (filter.OwnerId.HasValue && filter.IsPublicOrOwner == true)
            query = query.Where(r => r.OwnerId == filter.OwnerId || r.IsPublic);
        else if (filter.OwnerId.HasValue)
            query = query.Where(r => r.OwnerId == filter.OwnerId);

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filter.Name}%"));

        return query
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
            .ToArray();
    }

    /// <summary>
    /// Find the report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Report? FindById(int id)
    {
        return this.Context.Reports
            .Include(r => r.Owner)
            .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
            .Include(r => r.Sections).ThenInclude(s => s.Filter)
            .Include(r => r.Sections).ThenInclude(s => s.Folder)
            .Include(r => r.Sections).ThenInclude(s => s.ChartTemplatesManyToMany).ThenInclude(c => c.ChartTemplate)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Include(r => r.Events).ThenInclude(s => s.Schedule)
            .FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Get the current instance for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId">The owner of the instance.</param>
    /// <param name="limit">Number of instances to return.</param>
    /// <returns></returns>
    public IEnumerable<ReportInstance> GetLatestInstances(int id, int? ownerId = null, int limit = 2)
    {
        var query = this.Context.ReportInstances
            .Include(ri => ri.Owner)
            .Where(ri => ri.ReportId == id);

        if (ownerId.HasValue)
            query = query.Where(ri => ri.OwnerId == ownerId);

        return query
            .OrderByDescending(ri => ri.Id)
            .Take(limit)
            .ToArray();
    }

    /// <summary>
    /// Add the new report to the database.
    /// Add subscribers to the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Report Add(Report entity)
    {
        this.Context.AddRange(entity.SubscribersManyToMany);
        if (entity.Template != null)
        {
            if (entity.Template.Id == 0)
            {
                this.Context.Add(entity.Template);
                this.Context.Add(entity.Template.ChartTemplates);
                this.Context.Add(entity.Template.ChartTemplatesManyToMany);
            }
            else
            {
                this.Context.Entry(entity.Template).State = EntityState.Modified;
            }
        }
        this.Context.AddRange(entity.Sections);

        entity.Events.ForEach(reportEvent =>
        {
            if (reportEvent.Schedule != null)
            {
                this.Context.Add(reportEvent.Schedule);
                this.Context.Add(reportEvent);
            }
        });
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report in the database.
    /// Update subscribers of the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Report Update(Report entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");

        // Add/Update/Delete report subscribers.
        var originalSubscribers = original.SubscribersManyToMany.ToArray();
        originalSubscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(s =>
        {
            var originalSubscriber = originalSubscribers.FirstOrDefault(rs => rs.UserId == s.UserId);
            if (originalSubscriber == null)
                original.SubscribersManyToMany.Add(s);
            else
            {
                if (originalSubscriber.IsSubscribed != s.IsSubscribed)
                    originalSubscriber.IsSubscribed = s.IsSubscribed;
            }
        });

        var originalEvents = original.Events.ToArray();
        originalEvents.Except(entity.Events).ForEach(reportEvent =>
        {
            this.Context.Entry(reportEvent).State = EntityState.Deleted;
        });
        entity.Events.ForEach(reportEvent =>
        {
            var originalEvent = originalEvents.FirstOrDefault(s => s.Id == reportEvent.Id);
            if (originalEvent == null)
                original.Events.Add(reportEvent);
            else
            {
                originalEvent.Name = reportEvent.Name;
                originalEvent.Description = reportEvent.Description;
                originalEvent.IsEnabled = reportEvent.IsEnabled;
                originalEvent.RequestSentOn = reportEvent.RequestSentOn;
                originalEvent.LastRanOn = reportEvent.LastRanOn;
                originalEvent.EventType = reportEvent.EventType;
                originalEvent.Settings = reportEvent.Settings;
                originalEvent.ReportId = reportEvent.ReportId;
                originalEvent.NotificationId = reportEvent.NotificationId;
                originalEvent.ScheduleId = reportEvent.ScheduleId;
                if (originalEvent.Schedule != null && reportEvent.Schedule != null)
                {
                    originalEvent.Schedule.Name = reportEvent.Schedule.Name;
                    originalEvent.Schedule.Description = reportEvent.Schedule.Description;
                    originalEvent.Schedule.IsEnabled = reportEvent.Schedule.IsEnabled;
                    originalEvent.Schedule.DelayMS = reportEvent.Schedule.DelayMS;
                    originalEvent.Schedule.RunOn = reportEvent.Schedule.RunOn;
                    originalEvent.Schedule.StartAt = reportEvent.Schedule.StartAt;
                    originalEvent.Schedule.StopAt = reportEvent.Schedule.StopAt;
                    originalEvent.Schedule.RunOnlyOnce = reportEvent.Schedule.RunOnlyOnce;
                    originalEvent.Schedule.Repeat = reportEvent.Schedule.Repeat;
                    originalEvent.Schedule.RunOnWeekDays = reportEvent.Schedule.RunOnWeekDays;
                    originalEvent.Schedule.RunOnMonths = reportEvent.Schedule.RunOnMonths;
                    originalEvent.Schedule.DayOfMonth = reportEvent.Schedule.DayOfMonth;
                    originalEvent.Schedule.RequestedById = reportEvent.Schedule.RequestedById;
                }
            }
        });

        // Add/Update the report template.
        if (entity.Template != null)
        {
            if (entity.Template.Id == 0)
            {
                // A new template has been provided.
                this.Context.Add(entity.Template);
                original.Template = entity.Template;
            }
            else
            {
                if (original.Template == null)
                {
                    // A different template has been assigned.
                    if (original.TemplateId == entity.TemplateId)
                        this.Context.Entry(entity.Template).State = EntityState.Modified;
                    else
                        this.Context.Entry(entity.Template).State = EntityState.Added;

                    original.Template = entity.Template;
                }
                else if (original.TemplateId == entity.TemplateId)
                {
                    // Update the existing template.
                    original.Template.Name = entity.Template.Name;
                    original.Template.Description = entity.Template.Description;
                    original.Template.SortOrder = entity.Template.SortOrder;
                    original.Template.IsEnabled = entity.Template.IsEnabled;
                    original.Template.Subject = entity.Template.Subject;
                    original.Template.Body = entity.Template.Body;
                    original.Template.Settings = entity.Template.Settings;
                    original.Template.Version = entity.Template.Version;
                }
                else
                {
                    // A different template is now associated to this report.
                    this.Context.Entry(entity.Template).State = EntityState.Modified;
                    original.Template = entity.Template;
                }
            }
        }

        // Add/Update/Delete report sections.
        var originalSections = original.Sections.ToArray();
        originalSections.Except(entity.Sections).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.Sections.ForEach(updatedSection =>
        {
            var originalSection = originalSections.FirstOrDefault(rs => rs.Id == updatedSection.Id);
            if (originalSection == null || updatedSection.Id == 0)
            {
                original.Sections.Add(updatedSection);
                this.Context.AddRange(updatedSection.ChartTemplatesManyToMany);
            }
            else
            {
                originalSection.Name = updatedSection.Name;
                originalSection.Description = updatedSection.Description;
                originalSection.IsEnabled = updatedSection.IsEnabled;
                originalSection.SortOrder = updatedSection.SortOrder;
                originalSection.Settings = updatedSection.Settings;
                originalSection.ReportId = updatedSection.ReportId;
                originalSection.FilterId = updatedSection.FilterId;
                originalSection.FolderId = updatedSection.FolderId;
                originalSection.Version = updatedSection.Version;

                // Add/Update/Delete report section chart templates.
                var originalChartTemplates = originalSection.ChartTemplatesManyToMany.ToArray();
                originalChartTemplates.Except(updatedSection.ChartTemplatesManyToMany).ForEach(c =>
                {
                    this.Context.Entry(c).State = EntityState.Deleted;
                });
                updatedSection.ChartTemplatesManyToMany.ForEach(ct =>
                {
                    var originalChartTemplate = originalChartTemplates.FirstOrDefault(m => m.ChartTemplateId == ct.ChartTemplateId);
                    if (originalChartTemplate == null || originalChartTemplate.ChartTemplateId == 0)
                        this.Context.Add(ct);
                    else
                    {
                        originalChartTemplate.SortOrder = ct.SortOrder;
                        originalChartTemplate.Settings = ct.Settings;
                    }
                });
            }
        });

        original.Name = entity.Name;
        original.Description = entity.Description;
        original.IsEnabled = entity.IsEnabled;
        original.SortOrder = entity.SortOrder;
        original.IsPublic = entity.IsPublic;
        original.TemplateId = entity.TemplateId;
        original.OwnerId = entity.OwnerId;
        original.Settings = entity.Settings;
        original.Version = entity.Version;
        this.Context.ResetVersion(original);

        return base.Update(original);
    }

    /// <summary>
    /// Delete report and related entities.
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(Report entity)
    {
        var schedules = this.Context.EventSchedules
            .Include(es => es.Schedule)
            .Where(es => es.ReportId == entity.Id)
            .Select(es => es.Schedule!)
            .ToArray();

        this.Context.RemoveRange(schedules);
        base.Delete(entity);
    }

    /// <summary>
    /// Find the last report instance created for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private ReportInstance? GetPreviousReportInstance(int reportId, int? ownerId)
    {
        return this.Context.ReportInstances
            .Include(i => i.ContentManyToMany)
            .OrderByDescending(i => i.Id)
            .Where(i => i.ReportId == reportId &&
                i.OwnerId == ownerId)
            .FirstOrDefault();
    }

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// Makes a request for each section.
    /// If the section also references a folder it will make a request for the folder content too.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(Report report, int? requestorId)
    {
        var searchResults = new Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>();
        var reportSettings = JsonSerializer.Deserialize<ReportSettingsModel>(report.Settings.ToJson(), _serializerOptions) ?? new();

        var ownerId = requestorId ?? report.OwnerId; // TODO: Handle users generating instances for a report they do not own.
        var prevInstance = GetPreviousReportInstance(report.Id, ownerId);

        // Fetch the current instance of this report to exclude any content within it.
        var excludeHistoricalContentIds = reportSettings.Content.ExcludeHistorical ? this.GetReportInstanceContentToExclude(report.Id, ownerId) : Array.Empty<long>();

        // Fetch other reports to exclude any content within them.
        var excludeReportContentIds = reportSettings.Content.ExcludeReports.Any()
            ? reportSettings.Content.ExcludeReports.SelectMany((reportId) => this.GetReportInstanceContentToExclude(reportId, ownerId)).Distinct()
            : Array.Empty<long>();

        var excludeContentIds = excludeHistoricalContentIds.AppendRange(excludeReportContentIds).Distinct();
        var excludeAboveSectionContentIds = new List<long>();

        foreach (var section in report.Sections)
        {
            var sectionSettings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings.ToJson(), _serializerOptions) ?? new();

            // Content in a folder is added first.
            if (section.FolderId.HasValue)
            {
                var query = this.Context.FolderContents
                    .Include(fc => fc.Content)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Source)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Series)
                    .Include(fc => fc.Content).ThenInclude(c => c!.MediaType)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Contributor)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Owner)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Labels)
                    .Include(fc => fc.Content).ThenInclude(c => c!.FileReferences)
                    .Include(fc => fc.Content).ThenInclude(c => c!.TimeTrackings)
                    .Include(fc => fc.Content).ThenInclude(c => c!.Tags)
                    .Include(fc => fc.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(t => t.Action)
                    .Include(fc => fc.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(t => t.Action)
                    .Include(fc => fc.Content).ThenInclude(c => c!.TopicsManyToMany).ThenInclude(t => t.Topic)
                    .Include(fc => fc.Content).ThenInclude(c => c!.TonePoolsManyToMany).ThenInclude(t => t.TonePool)
                    .Where(fc => fc.FolderId == section.FolderId);

                if (sectionSettings.RemoveDuplicates)
                    query = query.Where(fc => !excludeAboveSectionContentIds.Contains(fc.ContentId));

                if (excludeContentIds.Any())
                    query = query.Where(fc => !excludeContentIds.Contains(fc.ContentId));

                var content = query
                    .OrderBy(fc => fc.SortOrder)
                    .ToArray();

                var folderContent = new Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>();
                folderContent.Hits.Hits = content
                    .Select(c => new Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>()
                    {
                        Source = new API.Areas.Services.Models.Content.ContentModel(c.Content!, _serializerOptions)
                    });
                searchResults.Add(section.Name, folderContent);
                excludeAboveSectionContentIds.AddRange(content.Select(c => c.ContentId).ToArray());
            }
            // Content in a filter is added second.
            if (section.FilterId.HasValue)
            {
                if (section.Filter == null) throw new InvalidOperationException($"Section '{section.Name}' filter is missing from report object.");

                // Modify the query to exclude content.
                var query = excludeContentIds.Any() ? section.Filter.Query.AddExcludeContent(excludeContentIds) : section.Filter.Query;

                // Only include content that has been posted since the last report instance.
                if (reportSettings.Content.OnlyNewContent)
                    query = query.IncludeOnlyLatestPosted(prevInstance?.PublishedOn);

                // Determine index.
                var searchUnpublished = section.Filter.Settings.GetElementValue(".searchUnpublished", false);
                var defaultIndex = searchUnpublished ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex;

                var content = await _elasticClient.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(defaultIndex, query);

                if (sectionSettings.RemoveDuplicates)
                    content.Hits.Hits = content.Hits.Hits.Where(c => !excludeAboveSectionContentIds.Contains(c.Source.Id));

                if (excludeContentIds.Any())
                    content.Hits.Hits = content.Hits.Hits.Where(c => !excludeContentIds.Contains(c.Source.Id));

                // Fetch custom content versions for the requestor.
                var contentIds = content.Hits.Hits.Select(h => h.Source).Select(c => c.Id).Distinct().ToArray();
                var results = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => new { c.Id, c.Versions }).ToArray();
                content.Hits.Hits.ForEach(h =>
                {
                    h.Source.Versions = results.FirstOrDefault(r => r.Id == h.Source.Id)?.Versions ?? new();
                });

                searchResults.Add(section.Name, content);
                excludeAboveSectionContentIds.AddRange(content.Hits.Hits.Select(c => c.Source.Id).ToArray());
            }
        }

        return searchResults;
    }

    /// <summary>
    /// Get the content from the current report instance for the specified 'reportId' and 'ownerId'.
    /// Including the 'ownerId' ensures the report the user generates is coupled with prior instances for the same user.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IEnumerable<long> GetReportInstanceContentToExclude(int reportId, int? ownerId)
    {
        var instance = GetPreviousReportInstance(reportId, ownerId);
        return instance?.ContentManyToMany.Select(c => c.ContentId).ToArray() ?? Array.Empty<long>();
    }

    /// <summary>
    /// Get the content from the related report instances for the specified 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public IEnumerable<long> GetRelatedReportInstanceContentToExclude(int reportId)
    {
        var report = this.Context.Reports.FirstOrDefault(r => r.Id == reportId) ?? throw new NoContentException("Report does not exist.");
        var relatedReportIds = report.Settings.GetElementValue("instance.excludeReports", Array.Empty<int>())!;

        var contentIds = new List<long>();
        relatedReportIds.ForEach(id =>
        {
            // Get the current instance of each related report and extract the content in it.
            contentIds.AddRange(this.Context.ReportInstances
                .Include(i => i.ContentManyToMany)
                .OrderByDescending(i => i.Id)
                .Where(i => i.ReportId == id)
                .SelectMany(i => i.ContentManyToMany.Select(c => c.ContentId))
                .ToArray());
        });

        return contentIds.Distinct().ToArray();
    }

    /// <summary>
    /// Clears all content from all folders in any section of the specified 'report'.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public Report? ClearFoldersInReport(Report report)
    {
        report.Sections.Where(s => s.Folder != null).ForEach(section =>
        {
            var folderContent = this.Context.FolderContents.Where(fc => fc.FolderId == section.FolderId).ToArray();
            this.Context.RemoveRange(folderContent);
        });

        this.Context.SaveChanges();

        return FindById(report.Id);
    }

    /// <summary>
    /// Subscribe the specified 'userId' to the specified report.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task<int> Subscribe(int userId, int reportId)
    {
        var saveChanges = false;
        var targetReport = FindById(reportId) ?? throw new NoContentException("Report does not exist");
        var subscriberRecord = targetReport.Subscribers.FirstOrDefault(s => s.Id == userId);
        if (subscriberRecord != null)
        {
            var userReports = this.Context.UserReports.Where(x => x.UserId == userId && x.ReportId == reportId);
            userReports.ForEach(s =>
            {
                if (s.IsSubscribed)
                {
                    s.IsSubscribed = true;
                    if (!saveChanges) saveChanges = true;
                }
            });
        }
        else
        {
            this.Context.UserReports.Add(new UserReport(userId, reportId, true));
            saveChanges = true;
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }
    /// <summary>
    /// Unsubscribe the specified 'userId' to the specified report.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task<int> Unsubscribe(int userId, int reportId)
    {
        var saveChanges = false;
        var targetReport = FindById(reportId) ?? throw new NoContentException("Report does not exist");
        var subscriberRecord = targetReport.Subscribers.FirstOrDefault(s => s.Id == userId);
        if (subscriberRecord != null)
        {
            var userReports = this.Context.UserReports.Where(x => x.UserId == userId && x.ReportId == reportId);
            userReports.ForEach(s =>
            {
                if (s.IsSubscribed)
                {
                    s.IsSubscribed = false;
                    if (!saveChanges) saveChanges = true;
                }
            });
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    public async Task<int> Unsubscribe(int userId)
    {
        var saveChanges = false;
        var userReports = this.Context.UserReports.Where(x => x.UserId == userId);
        userReports.ForEach(s =>
        {
            if (s.IsSubscribed)
            {
                s.IsSubscribed = !s.IsSubscribed;
                if (!saveChanges) saveChanges = true;
            }
        });
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }
    #endregion
}
