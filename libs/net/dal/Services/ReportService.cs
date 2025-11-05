using System;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Elastic;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Variables
    private readonly ElasticOptions _elasticOptions;
    private readonly ITNOElasticClient _elasticClient;
    private readonly IReportInstanceService _reportInstanceService;
    private readonly JsonSerializerOptions _serializerOptions;
    private static readonly TimeSpan DuplicateTitleWindow = TimeSpan.FromDays(3);
    #endregion

    #region Constructors
    public ReportService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IReportInstanceService reportInstanceService,
        IServiceProvider serviceProvider,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _reportInstanceService = reportInstanceService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
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
        if (filter.IsEnabled.HasValue)
            query = query.Where(r => r.IsEnabled);

        if (filter.OwnerId.HasValue && filter.IsPublicOrOwner == true)
            query = query.Where(r => r.OwnerId == filter.OwnerId || r.IsPublic);
        else if (filter.OwnerId.HasValue)
            query = query.Where(r => r.OwnerId == filter.OwnerId);
        if (filter.SubscriberUserId.HasValue)
            query = query.Where(n => n.SubscribersManyToMany.Any(ns => ns.IsSubscribed && ns.UserId == filter.SubscriberUserId.Value));

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filter.Name}%"));

        if (filter.Ids?.Any() == true)
            query = query.Where(r => filter.Ids.Contains(r.Id));

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(q => q.SortOrder).ThenBy(u => u.Name);

        if (filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

        return query.AsSplitQuery().ToArray();
    }

    /// <summary>
    /// Find the report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Report? FindById(int id)
    {
        // If you edit this function, also edit the related function in ReportEngine.OrderBySectionField
        var report = this.Context.Reports
            .Include(r => r.Owner)
            .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
            .Include(r => r.Sections).ThenInclude(s => s.Filter)
            .Include(r => r.Sections).ThenInclude(s => s.Folder)
            .Include(r => r.Sections).ThenInclude(s => s.ChartTemplatesManyToMany).ThenInclude(c => c.ChartTemplate)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Include(r => r.Events).ThenInclude(s => s.Schedule)
            .AsSplitQuery()
            .FirstOrDefault(r => r.Id == id);

        // Reorganize sections.
        var sections = report?.Sections.OrderBy(s => s.SortOrder).ToArray() ?? Array.Empty<ReportSection>();
        report?.Sections.Clear();
        report?.Sections.AddRange(sections);
        return report;
    }

    /// <summary>
    /// Get reports based on the filter for the dashboard.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Report> GetDashboard(DashboardFilter filter)
    {
        var query = this.Context.Reports
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Events).ThenInclude(e => e.Schedule)
            .AsQueryable();

        if (filter.IsEnabled.HasValue)
            query = query.Where(r => r.IsEnabled == filter.IsEnabled.Value);
        if (filter.IsPublic.HasValue)
            query = query.Where(r => r.IsPublic == filter.IsPublic.Value);
        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(c =>
                EF.Functions.Like(c.Name.ToLower(), $"%{filter.Keyword.ToLower()}%") ||
                EF.Functions.Like(c.Owner!.Username.ToLower(), $"%{filter.Keyword.ToLower()}%") ||
                EF.Functions.Like(c.Owner!.Email.ToLower(), $"%{filter.Keyword.ToLower()}%") ||
                EF.Functions.Like(c.Owner!.PreferredEmail.ToLower(), $"%{filter.Keyword.ToLower()}%") ||
                EF.Functions.Like(c.Owner!.FirstName.ToLower(), $"%{filter.Keyword.ToLower()}%") ||
                EF.Functions.Like(c.Owner!.LastName.ToLower(), $"%{filter.Keyword.ToLower()}%"));
        if (filter.OwnerId.HasValue)
            query = query.Where(r => r.OwnerId == filter.OwnerId.Value);
        if (filter.Status?.Any() == true)
            query = query.Where(r => r.Instances.OrderByDescending(i => i.Id).Take(1).Any(i => filter.Status.Contains(i.Status)));
        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            query = query.Where(r => r.Instances.OrderByDescending(i => i.Id).Take(1).Any(i => i.SentOn >= filter.StartDate.Value && i.SentOn < filter.EndDate.Value));
        else if (filter.StartDate.HasValue)
            query = query.Where(r => r.Instances.OrderByDescending(i => i.Id).Take(1).Any(i => i.SentOn >= filter.StartDate.Value));
        else if (filter.EndDate.HasValue)
            query = query.Where(r => r.Instances.OrderByDescending(i => i.Id).Take(1).Any(i => i.SentOn < filter.EndDate.Value));

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 10;
        var skip = (page - 1) * quantity;
        var reports = query
            .OrderBy(r => r.Name)
            .Skip(skip)
            .Take(quantity)
            .ToArray();

        // Fetch only the most recent report instances for each report.
        var reportIds = reports.Select(r => r.Id).ToArray();
        var instances = (
            from ri in this.Context.ReportInstances
            where reportIds.Contains(ri.ReportId)
            group ri by ri.ReportId into rig
            select rig.OrderByDescending(ri => ri.Id).Take(2))
            .AsNoTracking()
            .ToArray();

        foreach (var report in reports)
        {
            var reportInstances = instances.FirstOrDefault(i => i.Any(i2 => i2.ReportId == report.Id)) ?? Array.Empty<ReportInstance>();
            report.Instances.AddRange(reportInstances);
        }

        return reports;
    }

    /// <summary>
    /// Get the specified report for the dashboard.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Report GetDashboardReport(int id)
    {
        var report = this.Context.Reports
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Events).ThenInclude(e => e.Schedule)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User).ThenInclude(u => u!.Distribution).ThenInclude(d => d.LinkedUser)
            .Where(r => r.Id == id)
            .FirstOrDefault() ?? throw new NoContentException();

        var instances = (
            from ri in this.Context.ReportInstances
            where ri.ReportId == id
            select ri)
            .AsNoTracking()
            .OrderByDescending(ri => ri.Id)
            .Take(2)
            .ToArray();

        var instanceId = instances.Take(1).Select(ri => ri.Id).FirstOrDefault();
        var emails = (
            from uri in this.Context.UserReportInstances.Include(m => m.User).ThenInclude(u => u!.Distribution).ThenInclude(d => d.LinkedUser)
            where uri.InstanceId == instanceId
            select uri
        ).AsNoTracking().ToArray();

        foreach (var instance in instances)
        {
            instance.UserInstances.AddRange(emails.Where(e => e.InstanceId == instance.Id));
        }
        report.Instances.AddRange(instances);

        return report;
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
                this.Context.ChartTemplates.AddRange(entity.Template.ChartTemplates);
                this.Context.ReportTemplateChartTemplates.AddRange(entity.Template.ChartTemplatesManyToMany);
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
                if (originalSubscriber.Format != s.Format)
                    originalSubscriber.Format = s.Format;
                if (originalSubscriber.SendTo != s.SendTo)
                    originalSubscriber.SendTo = s.SendTo;
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

                // A filter/folder cannot be added this way and it may be used in a separate section.
                if (updatedSection.Filter != null) updatedSection.Filter = null;
                if (updatedSection.Folder != null) updatedSection.Folder = null;
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
                if (updatedSection.Filter != null && originalSection.Filter != null && updatedSection.Filter.OwnerId != originalSection.Filter?.OwnerId)
                {
                    originalSection.Filter!.OwnerId = updatedSection.Filter.OwnerId;
                    this.Context.Update(originalSection.Filter);
                }
                originalSection.FolderId = updatedSection.FolderId;
                if (updatedSection.Folder != null && originalSection.Folder != null && updatedSection.Folder.OwnerId != originalSection.Folder?.OwnerId)
                {
                    originalSection.Folder!.OwnerId = updatedSection.Folder.OwnerId;
                    this.Context.Update(originalSection.Folder);
                }
                originalSection.LinkedReportId = updatedSection.LinkedReportId;
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
    /// Generate an instance of the report.
    /// Populate the instance with content based on filters and folders.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestorId"></param>
    /// <param name="instanceId"></param>
    /// <param name="regenerate"></param>
    /// <returns></returns>
    public async Task<ReportInstance> GenerateReportInstanceAsync(
        int id,
        int? requestorId = null,
        long? instanceId = null,
        bool regenerate = false)
    {
        // Fetch content for every section within the report.  This will include folders and filters.
        var report = FindById(id) ?? throw new NoContentException("Report does not exist");
        var reportSettings = JsonSerializer.Deserialize<ReportSettingsModel>(report.Settings, _serializerOptions) ?? new ReportSettingsModel();
        List<ReportInstanceContent> instanceContent;
        if (reportSettings.Content.CopyPriorInstance && !regenerate)
        {
            var currentInstance = GetCurrentReportInstance(report.Id, requestorId) ?? new ReportInstance(report.Id);
            instanceContent = currentInstance.ContentManyToMany.Select(c => new ReportInstanceContent(0, c.ContentId, c.SectionName, c.SortOrder)).ToList();
        }
        else
        {
            var searchResults = await FindContentWithElasticsearchAsync(report, instanceId, requestorId);
            instanceContent = new List<ReportInstanceContent>(searchResults.SelectMany(sr => sr.Value.Hits.Hits).Count());
            report.Sections.ForEach(section =>
            {
                if (searchResults.TryGetValue(section.Name, out Elastic.Models.SearchResultModel<TNO.API.Areas.Services.Models.Content.ContentModel>? results))
                {
                    // Apply the search results to the report instance.
                    var settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings, _serializerOptions);
                    var sortOrder = 0;
                    instanceContent.AddRange(OrderBySectionField(results.Hits.Hits.Select(c =>
                    {
                        var content = c.Source.ToContent();
                        return new ReportInstanceContent(instanceId ?? 0, c.Source.Id, section.Name, sortOrder++)
                        {
                            Content = content
                        };
                    }), settings?.SortBy, settings?.SortDirection));
                }
            });
        }

        if (reportSettings.Content.RemoveDuplicateTitles3Days)
            instanceContent = FilterDuplicateTitlesWithinWindow(instanceContent, DuplicateTitleWindow);

        return new ReportInstance(
            instanceId ?? 0,
            id,
            requestorId ?? report.OwnerId,
            instanceContent
        )
        {
            OwnerId = requestorId,
            PublishedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Regenerate the content for the current report instance for the specified report 'id' and 'sectionId'.
    /// This provides a way to only refresh a single section within a report.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ReportInstance> RegenerateReportInstanceSectionAsync(int id, int sectionId, int? requestorId = null)
    {
        // Fetch content for every section within the report.  This will include folders and filters.
        var report = FindById(id) ?? throw new NoContentException("Report does not exist");
        var section = this.Context.ReportSections.FirstOrDefault(rs => rs.Id == sectionId) ?? throw new InvalidOperationException("Report section does not exist");

        var ownerId = requestorId ?? report.OwnerId; // TODO: Handle users generating instances for a report they do not own.
        var currentInstance = GetCurrentReportInstance(report.Id, ownerId, true) ?? new ReportInstance(id, ownerId)
        {
            PublishedOn = DateTime.UtcNow
        };
        currentInstance.Report = report;

        var searchResults = await FindContentWithElasticsearchAsync(currentInstance, section, requestorId);
        var instanceContent = new List<ReportInstanceContent>(searchResults.SelectMany(sr => sr.Value.Hits.Hits).Count());
        report.Sections.ForEach(section =>
        {
            var sortOrder = 0;
            if (searchResults.TryGetValue(section.Name, out Elastic.Models.SearchResultModel<TNO.API.Areas.Services.Models.Content.ContentModel>? results))
            {
                // Apply the search results to the report instance.
                var settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings, _serializerOptions);
                instanceContent.AddRange(
                    OrderBySectionField(
                        results.Hits.Hits.Select(c => new ReportInstanceContent(
                            currentInstance.Id,
                            c.Source.Id,
                            section.Name,
                            currentInstance.ContentManyToMany.FirstOrDefault(ci => ci.SectionName == section.Name && ci.ContentId == c.Source.Id)?.SortOrder ?? sortOrder++)
                        {
                            Content = c.Source?.ToContent()
                        }),
                    settings?.SortBy, settings?.SortDirection));
            }
            sortOrder = instanceContent.Any() ? instanceContent.Last().SortOrder + 1 : 0;
        });

        currentInstance.ContentManyToMany.Clear();
        currentInstance.ContentManyToMany.AddRange(instanceContent);
        return currentInstance;
    }

    /// <summary>
    /// Order the content based on the section settings field.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="sortBy"></param>
    /// <param name="direction"></param>
    /// <returns>Ordered Content</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ReportInstanceContent[] OrderBySectionField(IEnumerable<ReportInstanceContent> content, string? sortBy, string? direction)
    {
        // If you edit this function also edit the one in the ReportEngine.OrderBySectionField.
        IEnumerable<ReportInstanceContent> results;

        if (direction == "desc")
        {
            results = sortBy switch
            {
                "Headline" => content.OrderByDescending(c => c.Content?.Headline),
                "PublishedOn" => content.OrderByDescending(c => c.Content?.PublishedOn),
                "MediaType" => content.OrderByDescending(c => c.Content?.MediaType?.Name),
                "Series" => content.OrderByDescending(c => c.Content?.Series?.Name),
                "Source" => content.OrderByDescending(c => c.Content?.Source?.SortOrder).ThenByDescending(c => c.Content?.OtherSource),
                "Sentiment" => content.OrderByDescending(c => c.Content?.TonePoolsManyToMany.Select(s => s.Value).Sum(v => v)), // TODO: Support custom sentiment.
                "Byline" => content.OrderByDescending(c => c.Content?.Byline),
                "Contributor" => content.OrderByDescending(c => c.Content?.Contributor?.Name),
                "Topic" => content.OrderByDescending(c => string.Join(",", c.Content?.Topics.Select(x => x.Name) ?? Array.Empty<string>())),
                _ => content.OrderByDescending(c => c.SortOrder),
            };
        }
        else
        {
            results = sortBy switch
            {
                "Headline" => content.OrderBy(c => c.Content?.Headline),
                "PublishedOn" => content.OrderBy(c => c.Content?.PublishedOn),
                "MediaType" => content.OrderBy(c => c.Content?.MediaType?.Name),
                "Series" => content.OrderBy(c => c.Content?.Series?.Name),
                "Source" => content.OrderBy(c => c.Content?.Source?.SortOrder).ThenBy(c => c.Content?.OtherSource),
                "Sentiment" => content.OrderBy(c => c.Content?.TonePoolsManyToMany.Select(s => s.Value).Sum(v => v)), // TODO: Support custom sentiment.
                "Byline" => content.OrderBy(c => c.Content?.Byline),
                "Contributor" => content.OrderBy(c => c.Content?.Contributor?.Name),
                "Topic" => content.OrderBy(c => string.Join(",", c.Content?.Topics.Select(x => x.Name) ?? Array.Empty<string>())),
                _ => content.OrderBy(c => c.SortOrder),
            };
        }

        var sortOrder = 0;
        return results.Select(c =>
        {
            c.SortOrder = sortOrder++;
            return c;
        }).ToArray();
    }

    /// <summary>
    /// Add the specified 'content' to the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public async Task<Report?> AddContentToReportAsync(int id, int? ownerId, IEnumerable<ReportInstanceContent> content)
    {
        var report = FindById(id) ?? throw new NoContentException("Report does not exist");
        var instance = GetCurrentReportInstance(id, ownerId);
        if (instance == null)
        {
            // Create a new instance and populate it with the specified content.
            instance = await GenerateReportInstanceAsync(id, ownerId);

            // Add new content that does not already exist in the report and only for valid sections.
            var addContent = content.Where(c => report.Sections.Any(s => s.Name == c.SectionName) && !instance.ContentManyToMany.Any(ic => ic.SectionName == c.SectionName && ic.ContentId == c.ContentId));
            if (addContent.Any())
            {
                instance.ContentManyToMany.AddRange(addContent);

                instance = _reportInstanceService.AddAndSave(instance);
                report.Instances.Add(instance);
            }
        }
        else
        {
            // Add new content that does not already exist in the report and only for valid sections.
            var addContent = content.Where(c => report.Sections.Any(s => s.Name == c.SectionName) && !instance.ContentManyToMany.Any(ic => ic.SectionName == c.SectionName && ic.ContentId == c.ContentId));
            if (addContent.Any())
            {
                instance.ContentManyToMany.AddRange(addContent);
                instance = _reportInstanceService.UpdateAndSave(instance, true);
            }
        }

        return report;
    }

    /// <summary>
    /// Fast path to add content to an existing report instance without triggering a full graph update.
    /// Only inserts new ReportInstanceContent rows for the current instance and valid sections.
    /// If no instance exists it will generate a new instance first.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ReportContentMutation?> FastAddContentToReportAsync(int id, int? ownerId, IEnumerable<ReportInstanceContent> content)
    {
        if (!this.Context.Reports.AsNoTracking().Any(r => r.Id == id))
            throw new NoContentException("Report does not exist");

        var validSections = GetReportSectionNames(id);

        var instance = GetCurrentReportInstanceMetadata(id, ownerId);
        var instanceCreated = false;

        if (instance == null || IsInstanceLocked(instance))
        {
            // Fall back to the existing, full graph code path which will create a new instance
            // and persist all of the supplied content. This handles scenarios where we don't yet
            // have an instance (e.g. a brand-new report) as well as locked instances.
            _ = await AddContentToReportAsync(id, ownerId, content);

            instance = GetCurrentReportInstance(id, ownerId, includeContent: true)
                ?? throw new InvalidOperationException("Unable to create report instance for content append");

            var appendedFull = instance.ContentManyToMany
                .OrderBy(c => c.SectionName)
                .ThenBy(c => c.SortOrder)
                .ToArray();

            return new ReportContentMutation(id, instance, appendedFull, true);
        }

        var additions = content
            .Where(c => !string.IsNullOrWhiteSpace(c.SectionName) && validSections.Contains(c.SectionName))
            .Select(c => new ReportInstanceContent(instance.Id, c.ContentId, c.SectionName, c.SortOrder))
            .ToList();

        if (!additions.Any())
            return new ReportContentMutation(id, instance, Array.Empty<ReportInstanceContent>(), instanceCreated);

        var newContentIds = additions.Select(a => a.ContentId).Distinct().ToArray();
        var newSections = additions.Select(a => a.SectionName).Distinct().ToArray();

        var existingKeys = GetInstanceContentKeys(instance.Id, newContentIds, newSections);

        additions = additions
            .Where(a => !existingKeys.Contains((a.ContentId, a.SectionName)))
            .ToList();

        if (!additions.Any())
            return new ReportContentMutation(id, instance, Array.Empty<ReportInstanceContent>(), instanceCreated);

        var maxSortPerSection = this.Context.ReportInstanceContents
            .AsNoTracking()
            .Where(ric => ric.InstanceId == instance.Id && newSections.Contains(ric.SectionName))
            .GroupBy(ric => ric.SectionName)
            .Select(g => new { SectionName = g.Key, Max = (int?)g.Max(x => x.SortOrder) ?? 0 })
            .ToDictionary(x => x.SectionName, x => x.Max);

        foreach (var group in additions.GroupBy(a => a.SectionName))
        {
            var start = maxSortPerSection.TryGetValue(group.Key, out var max) ? max + 1 : 0;
            var offset = 0;
            foreach (var addition in group.OrderBy(a => a.SortOrder))
            {
                addition.SortOrder = start + offset++;
                addition.Content = null;
                addition.Instance = null;
            }
        }

        this.Context.ReportInstanceContents.AddRange(additions);
        this.CommitTransaction();

        var additionKeys = additions
            .Select(a => (a.ContentId, a.SectionName ?? string.Empty))
            .ToArray();

        var appended = _reportInstanceService
            .GetContentForInstance(instance.Id, additionKeys)
            .ToArray();

        return new ReportContentMutation(id, instance, appended, instanceCreated);
    }

    /// <summary>
    /// Get the latest instances for the specified report 'id' and 'ownerId'.
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
    /// Find the last report instance created for the specified report 'id' and 'ownerId'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <param name="includeContent"></param>
    /// <returns></returns>
    public ReportInstance? GetCurrentReportInstance(int id, int? ownerId = null, bool includeContent = false)
    {
        var query = this.Context.ReportInstances
            .AsNoTracking()
            .AsSplitQuery()
            .Where(i => i.ReportId == id);

        if (ownerId.HasValue == true)
            query = query.Where(ri => ri.OwnerId == ownerId);

        if (includeContent)
            query = query
                .Include(i => i.ContentManyToMany)
                    .ThenInclude(c => c.Content).ThenInclude(c => c!.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
                .Include(i => i.ContentManyToMany)
                    .ThenInclude(c => c.Content).ThenInclude(c => c!.MediaType)
                .Include(i => i.ContentManyToMany)
                    .ThenInclude(c => c.Content).ThenInclude(c => c!.TopicsManyToMany).ThenInclude(t => t.Topic);
        else
            query = query.Include(i => i.ContentManyToMany);

        return query.OrderByDescending(i => i.Id).FirstOrDefault();
    }

    /// <summary>
    /// Lightweight lookup for the latest report instance metadata without loading navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private ReportInstance? GetCurrentReportInstanceMetadata(int id, int? ownerId = null)
    {
        var query = this.Context.ReportInstances
            .AsNoTracking()
            .Where(i => i.ReportId == id);

        if (ownerId.HasValue)
            query = query.Where(ri => ri.OwnerId == ownerId);

        var instance = query
            .OrderByDescending(i => i.Id)
            .Select(ri => new
            {
                ri.Id,
                ri.ReportId,
                ri.OwnerId,
                ri.Status,
                ri.PublishedOn,
                ri.SentOn,
                ri.Subject,
                ri.Body,
                ri.Response,
                ri.CreatedOn,
                ri.CreatedBy,
                ri.UpdatedOn,
                ri.UpdatedBy,
                ri.Version,
            })
            .FirstOrDefault();

        if (instance == null) return null;

        var result = new ReportInstance(instance.Id, instance.ReportId, instance.OwnerId);
        result.Status = instance.Status;
        result.PublishedOn = instance.PublishedOn;
        result.SentOn = instance.SentOn;
        result.Subject = instance.Subject;
        result.Body = instance.Body;
        result.Response = instance.Response;
        result.CreatedOn = instance.CreatedOn;
        result.CreatedBy = instance.CreatedBy;
        result.UpdatedOn = instance.UpdatedOn;
        result.UpdatedBy = instance.UpdatedBy;
        result.Version = instance.Version;
        return result;
    }

    /// <summary>
    /// Fetch existing content keys for the specified instance using a single lightweight query.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="contentIds"></param>
    /// <param name="sectionNames"></param>
    /// <returns></returns>
    private HashSet<(long ContentId, string SectionName)> GetInstanceContentKeys(long instanceId, IEnumerable<long> contentIds, IEnumerable<string> sectionNames)
    {
        var idArray = contentIds?.Distinct().ToArray() ?? Array.Empty<long>();
        var sectionArray = sectionNames?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray() ?? Array.Empty<string>();
        if (idArray.Length == 0 || sectionArray.Length == 0) return new HashSet<(long, string)>();

        return this.Context.ReportInstanceContents
            .AsNoTracking()
            .Where(ric => ric.InstanceId == instanceId && idArray.Contains(ric.ContentId) && sectionArray.Contains(ric.SectionName))
            .Select(ric => new { ric.ContentId, ric.SectionName })
            .ToArray()
            .Select(x => (x.ContentId, x.SectionName))
            .ToHashSet();
    }
    private static bool IsInstanceLocked(ReportInstance instance)
    {
        return instance.Status >= ReportStatus.Accepted && instance.Status != ReportStatus.Reopen;
    }

    private HashSet<string> GetReportSectionNames(int reportId)
    {
        return this.Context.ReportSections
            .AsNoTracking()
            .Where(rs => rs.ReportId == reportId)
            .Select(rs => rs.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Find the previous report instance created for the specified report 'id' and 'ownerId'.
    /// Find the previous report instance that was sent.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="instanceId"></param>
    /// <param name="ownerId"></param>
    /// <param name="includeContent"></param>
    /// <param name="qty">Number of prior instances.</param>
    /// <returns></returns>
    public ReportInstance[] GetPreviousReportInstances(int id, long? instanceId, int? ownerId = null, bool includeContent = false, int qty = 2)
    {
        var query = this.Context.ReportInstances
            .AsNoTracking()
            .OrderByDescending(i => i.Id)
            .Where(i => i.ReportId == id
                && i.SentOn != null);

        if (instanceId.HasValue == true)
            query = query.Where(ri => ri.Id < instanceId);

        if (ownerId.HasValue == true)
            query = query.Where(ri => ri.OwnerId == ownerId);

        if (includeContent)
            query = query.Include(i => i.ContentManyToMany).ThenInclude(c => c.Content);
        else
            query = query.Include(i => i.ContentManyToMany);

        return query.Take(qty).ToArray();
    }

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// It will generate content for each section.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="instanceId"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(Report report, long? instanceId, int? requestorId)
    {
        var searchResults = new Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>();
        var reportSettings = JsonSerializer.Deserialize<ReportSettingsModel>(report.Settings.ToJson(), _serializerOptions) ?? new();

        var ownerId = requestorId ?? report.OwnerId; // TODO: Handle users generating instances for a report they do not own.
        var currentInstance = instanceId.HasValue ?
            this.Context.ReportInstances
                .AsNoTracking()
                .Include(ri => ri.ContentManyToMany)
                .Where(ri => ri.OwnerId == ownerId)
                .FirstOrDefault(ri => ri.Id == instanceId) :
            GetCurrentReportInstance(report.Id, ownerId);
        var previousInstances = GetPreviousReportInstances(report.Id, instanceId ?? currentInstance?.Id, ownerId);
        var instances = currentInstance?.SentOn.HasValue == true ? [currentInstance, .. previousInstances] : previousInstances;
        var previousInstance = instances.FirstOrDefault();

        // Create an array of content from the previous instance to exclude.
        var excludeHistoricalContentIds = reportSettings.Content.ExcludeHistorical
            ? [.. instances.SelectMany(pi => pi.ContentManyToMany.Select((c) => c.ContentId)).Distinct()]
            : Array.Empty<long>();

        // When an auto report runs it may need to exclude content in the currently unsent report.
        if (currentInstance != null && currentInstance.SentOn.HasValue == false
            && (reportSettings.Content.ExcludeContentInUnsentReport || !reportSettings.Content.ClearOnStartNewReport))
            excludeHistoricalContentIds = [.. excludeHistoricalContentIds.AppendRange(currentInstance.ContentManyToMany.Select(c => c.ContentId)).Distinct()];

        // Fetch other reports to exclude any content within them.
        var excludeReportContentIds = reportSettings.Content.ExcludeReports.Any()
            ? [.. reportSettings.Content.ExcludeReports.SelectMany((reportId) => this.GetReportInstanceContentToExclude(reportId, ownerId)).Distinct()]
            : Array.Empty<long>();

        var excludeAboveSectionContentIds = new List<long>();

        foreach (var section in report.Sections.OrderBy(s => s.SortOrder))
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

                if (!sectionSettings.OverrideExcludeHistorical)
                {
                    var excludeContentIds = excludeHistoricalContentIds.AppendRange(excludeReportContentIds).Distinct().ToArray();
                    if (excludeContentIds.Length != 0)
                        query = query.Where(fc => !excludeContentIds.Contains(fc.ContentId));
                }
                else if (excludeReportContentIds.Length != 0)
                    query = query.Where(fc => !excludeReportContentIds.Contains(fc.ContentId));

                var content = query
                    .OrderBy(fc => fc.SortOrder)
                    .ToArray();

                if (sectionSettings.RemoveDuplicateTitles3Days || reportSettings.Content.RemoveDuplicateTitles3Days)
                    content = FilterDuplicateTitlesWithinWindow(content, DuplicateTitleWindow);

                var folderContent = new Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>();
                folderContent.Hits.Hits = content
                    .Select(c => new Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>()
                    {
                        Source = new API.Areas.Services.Models.Content.ContentModel(c.Content!, _serializerOptions)
                    });
                searchResults.Add(section.Name, folderContent);
                var contentInThisFolder = content.Select(c => c.ContentId).ToArray();
                excludeAboveSectionContentIds.AddRange(contentInThisFolder.Except(excludeAboveSectionContentIds));
            }
            else if (section.FilterId.HasValue)
            {
                if (section.Filter == null) throw new InvalidOperationException($"Section '{section.Name}' filter is missing from report object.");
                var filterSettings = JsonSerializer.Deserialize<FilterSettingsModel>(section.Filter.Settings.ToJson(), _serializerOptions) ?? new();

                // Modify the query to exclude content.
                var excludeContentIds = excludeHistoricalContentIds.AppendRange(excludeReportContentIds).Distinct().ToArray();
                var excludeOnlyTheseContentIds = !sectionSettings.OverrideExcludeHistorical ? excludeContentIds : excludeReportContentIds;
                var excludeAboveAndHistorical = sectionSettings.RemoveDuplicates ? [.. excludeOnlyTheseContentIds.AppendRange(excludeAboveSectionContentIds).Distinct()] : excludeOnlyTheseContentIds;
                var query = excludeAboveAndHistorical.Length != 0
                    ? section.Filter.Query.AddExcludeContent(excludeAboveAndHistorical)
                    : section.Filter.Query;

                // Exclude sources and media types that the user cannot see.
                var excludeSources = this.Context.UserSources.Where(us => us.UserId == ownerId).Select(us => us.SourceId).ToArray();
                var excludeMediaTypes = this.Context.UserMediaTypes.Where(us => us.UserId == ownerId).Select(us => us.MediaTypeId).ToArray();
                query = query.AddExcludeSources(excludeSources);
                query = query.AddExcludeMediaTypes(excludeMediaTypes);

                // Only include content that has been posted since the last report instance.
                if (reportSettings.Content.OnlyNewContent)
                    query = query.IncludeOnlyLatestPosted(previousInstance?.PublishedOn);
                if (reportSettings.Content.OmitBCUpdates)
                    query = query.ExcludeBCUpdates();

                // Determine index.
                var defaultIndex = filterSettings.SearchUnpublished ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex;

                // Add retry logic for Elasticsearch query to handle failures
                var content = await RetryElasticsearchQueryAsync(async () =>
                    await _elasticClient.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(defaultIndex, query));
                if (sectionSettings.RemoveDuplicateTitles3Days || reportSettings.Content.RemoveDuplicateTitles3Days)
                    content.Hits.Hits = FilterDuplicateTitlesWithinWindow(
                        content.Hits.Hits ?? Array.Empty<Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>>(),
                        DuplicateTitleWindow);
                var contentHits = content.Hits.Hits.ToArray();

                // Fetch custom content versions for the requestor.
                var contentIds = content.Hits.Hits.Select(h => h.Source.Id).Distinct().ToArray();
                var results = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => new { c.Id, c.Versions }).ToArray();
                content.Hits.Hits.ForEach(h =>
                {
                    h.Source.Versions = results.FirstOrDefault(r => r.Id == h.Source.Id)?.Versions ?? new();
                });

                searchResults.Add(section.Name, content);
                excludeAboveSectionContentIds.AddRange(contentIds.Except(excludeAboveSectionContentIds));
            }
            else if (!reportSettings.Content.ClearOnStartNewReport
                && (previousInstance != null || currentInstance?.SentOn.HasValue == false))
            {
                // When a section has no folder or filter, keep the existing or prior report content in the new or updated instance.
                var copyInstance = previousInstance ?? currentInstance;
                if (copyInstance != null && copyInstance.ContentManyToMany.Any(c => c.SectionName == section.Name))
                {
                    var query = this.Context.ReportInstanceContents
                        .Include(ric => ric.Content)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Source)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Series)
                        .Include(ric => ric.Content).ThenInclude(c => c!.MediaType)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Contributor)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Owner)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Labels)
                        .Include(ric => ric.Content).ThenInclude(c => c!.FileReferences)
                        .Include(ric => ric.Content).ThenInclude(c => c!.TimeTrackings)
                        .Include(ric => ric.Content).ThenInclude(c => c!.Tags)
                        .Include(ric => ric.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(t => t.Action)
                        .Include(ric => ric.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(t => t.Action)
                        .Include(ric => ric.Content).ThenInclude(c => c!.TopicsManyToMany).ThenInclude(t => t.Topic)
                        .Include(ric => ric.Content).ThenInclude(c => c!.TonePoolsManyToMany).ThenInclude(t => t.TonePool)
                        .Where(ric => ric.InstanceId == copyInstance.Id && ric.SectionName == section.Name);

                    var content = query.Select(c => c).ToArray();
                    var sectionContent = new Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>();
                    sectionContent.Hits.Hits = content
                        .Select(c => new Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>()
                        {
                            Source = new API.Areas.Services.Models.Content.ContentModel(c.Content!, _serializerOptions)
                        });
                    searchResults.Add(section.Name, sectionContent);
                    var contentIds = sectionContent.Hits.Hits.Select(h => h.Source.Id).Distinct().ToArray();
                    excludeAboveSectionContentIds.AddRange(contentIds.Except(excludeAboveSectionContentIds));
                }
            }
        }

        return searchResults;
    }

    /// <summary>
    /// Retry Elasticsearch query once after 200ms to handle intermittent failures.
    /// This helps resolve issues like JSON parsing failures in BC Updates filtering.
    /// </summary>
    /// <typeparam name="T">The return type of the Elasticsearch operation</typeparam>
    /// <param name="operation">The Elasticsearch operation to retry</param>
    /// <returns>The result of the successful operation</returns>
    private async Task<T> RetryElasticsearchQueryAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex,
                "Elasticsearch query failed. Retrying in 200ms. Error: {ErrorMessage}",
                ex.Message);

            await Task.Delay(200);
            return await operation();
        }
    }

    /// <summary>
    /// Find content with Elasticsearch for the specified `reportInstance` and `section`.
    /// This method supports regenerating content within a section.
    /// The specified 'reportInstance' is treated like the current instance.
    /// </summary>
    /// <param name="reportInstance"></param>
    /// <param name="section"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(ReportInstance reportInstance, ReportSection section, int? requestorId)
    {
        var report = reportInstance.Report ?? throw new InvalidOperationException("Report instance must include report");
        var searchResults = new Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>();
        var reportSettings = JsonSerializer.Deserialize<ReportSettingsModel>(report.Settings.ToJson(), _serializerOptions) ?? new();

        var ownerId = requestorId ?? reportInstance.OwnerId; // TODO: Handle users generating instances for a report they do not own.
        var previousInstances = reportInstance != null ? GetPreviousReportInstances(report.Id, reportInstance.Id, ownerId) : null;
        var previousInstance = previousInstances?.FirstOrDefault();

        // Organize the content sections, and remove the specified section.
        var currentInstanceContent = reportInstance?.ContentManyToMany.Where(c => c.SectionName != section.Name).ToArray() ?? Array.Empty<ReportInstanceContent>();
        var contentAbove = new List<ReportInstanceContent>();
        report.Sections.OrderBy(s => s.SortOrder).ForEach(s =>
        {
            var sectionResults = new Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>();
            if (s.Name != section.Name)
            {
                var sectionContent = currentInstanceContent.Where(c => c.SectionName == s.Name).ToArray();
                if (s.SortOrder < section.SortOrder) contentAbove.AddRange(sectionContent);

                sectionResults.Hits.Hits = sectionContent
                    .Select(c => new Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>()
                    {
                        Source = new API.Areas.Services.Models.Content.ContentModel(c.Content!, _serializerOptions)
                    });
            }
            searchResults.Add(s.Name, sectionResults);
        });

        // Create an array of content from the previous instance to exclude from the report.
        var excludeHistoricalContentIds = reportSettings.Content.ExcludeHistorical && previousInstances?.Length > 0
            ? previousInstances?.SelectMany(pi => pi.ContentManyToMany.Select((c) => c.ContentId).ToArray()) ?? Array.Empty<long>()
            : Array.Empty<long>();

        // Fetch other reports to exclude any content within them.
        var excludeReportContentIds = reportSettings.Content.ExcludeReports.Any()
            ? reportSettings.Content.ExcludeReports.SelectMany((reportId) => this.GetReportInstanceContentToExclude(reportId, ownerId)).Distinct().ToArray()
            : Array.Empty<long>();

        var excludeContentIds = excludeHistoricalContentIds.AppendRange(excludeReportContentIds).Distinct().ToArray();
        var excludeAboveSectionContentIds = new List<long>();

        // Identify any content above that may need to be excluded.
        if (reportInstance != null)
            excludeAboveSectionContentIds.AddRange(contentAbove.Select(c => c.ContentId).ToArray());

        var sectionSettings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings.ToJson(), _serializerOptions) ?? new();

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

            if (excludeContentIds.Any() && !sectionSettings.OverrideExcludeHistorical)
                query = query.Where(fc => !excludeContentIds.Contains(fc.ContentId));

            var content = query
                .OrderBy(fc => fc.SortOrder)
                .ToArray();

            if (sectionSettings.RemoveDuplicateTitles3Days || reportSettings.Content.RemoveDuplicateTitles3Days)
                content = FilterDuplicateTitlesWithinWindow(content, DuplicateTitleWindow);

            var folderContent = new Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>();
            folderContent.Hits.Hits = content
                .Select(c => new Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>()
                {
                    Source = new API.Areas.Services.Models.Content.ContentModel(c.Content!, _serializerOptions)
                });
            searchResults[section.Name] = folderContent;
        }
        else if (section.FilterId.HasValue)
        {
            if (section.Filter == null) throw new InvalidOperationException($"Section '{section.Name}' filter is missing from report object.");
            var filterSettings = JsonSerializer.Deserialize<FilterSettingsModel>(section.Filter.Settings.ToJson(), _serializerOptions) ?? new();

            // Modify the query to exclude content.
            var excludeOnlyTheseContentIds = excludeContentIds.Any() && !sectionSettings.OverrideExcludeHistorical ? excludeContentIds : Array.Empty<long>();
            var excludeAboveAndHistorical = sectionSettings.RemoveDuplicates
                ? excludeOnlyTheseContentIds.AppendRange(excludeAboveSectionContentIds).Distinct().ToArray()
                : Array.Empty<long>();
            var query = excludeAboveAndHistorical.Any()
                ? section.Filter.Query.AddExcludeContent(excludeAboveAndHistorical)
                : section.Filter.Query;

            // Exclude sources and media types that the user cannot see.
            var excludeSources = this.Context.UserSources.Where(us => us.UserId == ownerId).Select(us => us.SourceId).ToArray();
            var excludeMediaTypes = this.Context.UserMediaTypes.Where(us => us.UserId == ownerId).Select(us => us.MediaTypeId).ToArray();
            query = query.AddExcludeSources(excludeSources);
            query = query.AddExcludeMediaTypes(excludeMediaTypes);

            // Only include content that has been posted since the last report instance.
            if (reportSettings.Content.OnlyNewContent && previousInstance?.PublishedOn.HasValue == true)
                query = query.IncludeOnlyLatestPosted(previousInstance.PublishedOn);

            // Determine index.
            var defaultIndex = filterSettings.SearchUnpublished ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex;
            var content = await RetryElasticsearchQueryAsync(async () =>
                await _elasticClient.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(defaultIndex, query));
            if (sectionSettings.RemoveDuplicateTitles3Days || reportSettings.Content.RemoveDuplicateTitles3Days)
                content.Hits.Hits = FilterDuplicateTitlesWithinWindow(
                    content.Hits.Hits ?? Array.Empty<Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>>(),
                    DuplicateTitleWindow);

            // Fetch custom content versions for the requestor.
            var contentIds = content.Hits.Hits.Select(h => h.Source.Id).Distinct().ToArray();
            var results = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => new { c.Id, c.Versions }).ToArray();
            content.Hits.Hits.ForEach(h =>
            {
                h.Source.Versions = results.FirstOrDefault(r => r.Id == h.Source.Id)?.Versions ?? new();
            });

            searchResults[section.Name] = content;
        }

        return searchResults;
    }

    private FolderContent[] FilterDuplicateTitlesWithinWindow(FolderContent[] items, TimeSpan window)
    {
        if (items.Length == 0) return items;

        var keep = new bool[items.Length];
        Array.Fill(keep, true);
        var threshold = DateTime.UtcNow - window;

        var metadata = items.Select((item, index) => new
        {
            Item = item,
            Index = index,
            Title = NormalizeHeadline(item.Content?.Headline),
            Date = GetComparableDate(item.Content)
        }).ToArray();

        foreach (var group in metadata
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => x.Title!, StringComparer.OrdinalIgnoreCase))
        {
            var recent = group.Where(x => x.Date >= threshold).ToArray();
            if (recent.Length <= 1) continue;

            var latest = recent
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Item.Content?.UpdatedOn ?? DateTime.MinValue)
                .ThenByDescending(x => x.Item.ContentId)
                .First();

            foreach (var entry in recent)
            {
                if (entry.Index != latest.Index)
                    keep[entry.Index] = false;
            }
        }

        var results = new List<FolderContent>(items.Length);
        for (var i = 0; i < items.Length; i++)
        {
            if (keep[i]) results.Add(items[i]);
        }

        return results.ToArray();
    }

    private Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>[] FilterDuplicateTitlesWithinWindow(
        IEnumerable<Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>> hits,
        TimeSpan window)
    {
        var hitArray = hits.ToArray();
        if (hitArray.Length == 0) return hitArray;

        var keep = new bool[hitArray.Length];
        Array.Fill(keep, true);
        var threshold = DateTime.UtcNow - window;

        var metadata = hitArray.Select((hit, index) => new
        {
            Hit = hit,
            Index = index,
            Title = NormalizeHeadline(hit.Source?.Headline),
            Date = GetComparableDate(hit.Source)
        }).ToArray();

        foreach (var group in metadata
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => x.Title!, StringComparer.OrdinalIgnoreCase))
        {
            var recent = group.Where(x => x.Date >= threshold).ToArray();
            if (recent.Length <= 1) continue;

            var latest = recent
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Hit.Source?.UpdatedOn ?? DateTime.MinValue)
                .ThenByDescending(x => x.Hit.Source?.Id ?? 0)
                .First();

            foreach (var entry in recent)
            {
                if (entry.Index != latest.Index)
                    keep[entry.Index] = false;
            }
        }

        var results = new List<Elastic.Models.HitModel<API.Areas.Services.Models.Content.ContentModel>>(hitArray.Length);
        for (var i = 0; i < hitArray.Length; i++)
        {
            if (keep[i]) results.Add(hitArray[i]);
        }

        return results.ToArray();
    }

    private List<ReportInstanceContent> FilterDuplicateTitlesWithinWindow(List<ReportInstanceContent> content, TimeSpan window)
    {
        if (content.Count == 0) return content;

        var keep = new bool[content.Count];
        Array.Fill(keep, true);
        var threshold = DateTime.UtcNow - window;

        var metadata = content.Select((item, index) => new
        {
            Item = item,
            Index = index,
            Title = NormalizeHeadline(item.Content?.Headline),
            Date = GetComparableDate(item.Content)
        }).ToArray();

        foreach (var group in metadata
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => x.Title!, StringComparer.OrdinalIgnoreCase))
        {
            var recent = group.Where(x => x.Date >= threshold).ToArray();
            if (recent.Length <= 1) continue;

            var latest = recent
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Item.Content?.UpdatedOn ?? DateTime.MinValue)
                .ThenByDescending(x => x.Item.ContentId)
                .First();

            foreach (var entry in recent)
            {
                if (entry.Index != latest.Index)
                    keep[entry.Index] = false;
            }
        }

        var results = new List<ReportInstanceContent>(content.Count);
        for (var i = 0; i < content.Count; i++)
        {
            if (keep[i]) results.Add(content[i]);
        }

        foreach (var sectionGroup in results.GroupBy(c => c.SectionName))
        {
            var order = 0;
            foreach (var item in sectionGroup.OrderBy(c => c.SortOrder))
                item.SortOrder = order++;
        }

        return results;
    }

    private static string? NormalizeHeadline(string? headline)
    {
        return string.IsNullOrWhiteSpace(headline) ? null : headline.Trim().ToUpperInvariant();
    }

    private static DateTime GetComparableDate(API.Areas.Services.Models.Content.ContentModel? content)
    {
        return content == null ? DateTime.MinValue : GetComparableDate(content.PublishedOn, content.PostedOn, content.UpdatedOn, content.CreatedOn);
    }

    private static DateTime GetComparableDate(Content? content)
    {
        return content == null ? DateTime.MinValue : GetComparableDate(content.PublishedOn, content.PostedOn, content.UpdatedOn, content.CreatedOn);
    }

    private static DateTime GetComparableDate(DateTime? publishedOn, DateTime? postedOn, DateTime? updatedOn, DateTime? createdOn)
    {
        return publishedOn ?? postedOn ?? updatedOn ?? createdOn ?? DateTime.MinValue;
    }

    /// <summary>
    /// Get the content from the current report instance for the specified 'reportId' and 'ownerId'.
    /// Including the 'ownerId' ensures the report the user generates is coupled with prior instances for the same user.
    /// Also get the previous report instances to exclude their content as well.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="previousInstancesQty"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IEnumerable<long> GetReportInstanceContentToExclude(int reportId, int? ownerId, int previousInstancesQty = 2)
    {
        var instance = GetCurrentReportInstance(reportId, ownerId);
        var previousInstances = GetPreviousReportInstances(reportId, instance?.Id, ownerId, false, previousInstancesQty);
        return instance?.ContentManyToMany.Select(c => c.ContentId).AppendRange(previousInstances?.SelectMany(pi => pi.ContentManyToMany.Select((c) => c.ContentId)) ?? []) ?? [];
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
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ReportService - ClearFoldersInReport Report Id: {report.Id} throws exception.");
            throw;
        }

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

    /// <summary>
    /// Unsubscribe the specified 'userId' from all reports.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Get all content for each report belonging to the specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Dictionary<int, long[]> GetAllContentInMyReports(int userId)
    {
        var reportIds = this.Context.Reports.Where(r => r.OwnerId == userId).Select(r => r.Id).ToArray();
        var result = new Dictionary<int, long[]>();

        foreach (var reportId in reportIds)
        {
            // Get current instance.
            long? instanceId = (from ri in this.Context.ReportInstances
                                where ri.ReportId == reportId
                                orderby ri.Id descending
                                select ri.Id).Take(1).FirstOrDefault();

            if (instanceId.HasValue)
            {
                // Get content in instance.
                var contentIds = (from ric in this.Context.ReportInstanceContents
                                  where ric.InstanceId == instanceId
                                  select ric.ContentId)
                        .Distinct()
                        .ToArray();

                result.Add(reportId, contentIds);
            }
        }

        return result;
    }

    /// <summary>
    /// get all CHES message Ids for reports at the specified 'status' and that were sent on or after 'cutOff' date and time.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="cutOff"></param>
    /// <returns></returns>
    public IEnumerable<API.Areas.Services.Models.Report.ChesReportMessagesModel> GetChesMessageIds(ReportStatus status, DateTime cutOff)
    {
        var linkReports = this.Context.UserReportInstances.Include(i => i.Instance).Where(r => r.LinkStatus == status && r.LinkSentOn >= cutOff).Select(r => new { r.Instance!.ReportId, r.InstanceId, r.UserId, SentOn = r.LinkSentOn, r.LinkResponse }).ToArray();
        var textReports = this.Context.UserReportInstances.Include(i => i.Instance).Where(r => r.TextStatus == status && r.TextSentOn >= cutOff).Select(r => new { r.Instance!.ReportId, r.InstanceId, r.UserId, SentOn = r.TextSentOn, r.TextResponse }).ToArray();
        var avReports = this.Context.UserAVOverviewInstances.Where(r => r.Status == status && r.SentOn >= cutOff).Select(r => new { r.InstanceId, r.UserId, r.SentOn, r.Response }).ToArray();

        var messages = new List<API.Areas.Services.Models.Report.ChesReportMessagesModel>();

        foreach (var report in linkReports)
        {
            var response = JsonSerializer.Deserialize<Ches.Models.EmailResponseModel>(report.LinkResponse.ToJson(), _serializerOptions);
            var messageIds = response?.Messages.Select(m => m.MessageId).ToArray() ?? [];
            messages.Add(new API.Areas.Services.Models.Report.ChesReportMessagesModel()
            {
                ReportType = ReportType.Content,
                Format = ReportDistributionFormat.LinkOnly,
                ReportId = report.ReportId,
                InstanceId = report.InstanceId,
                UserId = report.UserId,
                SentOn = report.SentOn,
                Status = status,
                MessageIds = messageIds,
            });
        }

        foreach (var report in textReports)
        {
            var response = JsonSerializer.Deserialize<Ches.Models.EmailResponseModel>(report.TextResponse.ToJson(), _serializerOptions);
            var messageIds = response?.Messages.Select(m => m.MessageId).ToArray() ?? [];
            messages.Add(new API.Areas.Services.Models.Report.ChesReportMessagesModel()
            {
                ReportType = ReportType.Content,
                Format = ReportDistributionFormat.FullText,
                ReportId = report.ReportId,
                InstanceId = report.InstanceId,
                UserId = report.UserId,
                SentOn = report.SentOn,
                Status = status,
                MessageIds = messageIds,
            });
        }

        foreach (var report in avReports)
        {
            var response = JsonSerializer.Deserialize<Ches.Models.EmailResponseModel>(report.Response.ToJson(), _serializerOptions);
            var messageIds = response?.Messages.Select(m => m.MessageId).ToArray() ?? [];
            messages.Add(new API.Areas.Services.Models.Report.ChesReportMessagesModel()
            {
                ReportType = ReportType.AVOverview,
                Format = ReportDistributionFormat.FullText,
                InstanceId = report.InstanceId,
                UserId = report.UserId,
                SentOn = report.SentOn,
                Status = status,
                MessageIds = messageIds,
            });
        }

        return messages;
    }

    /// <summary>
    /// Add the user report.
    /// </summary>
    /// <param name="subscription"></param>
    public void AddAndSave(UserReport subscription)
    {
        this.Context.Add(subscription);
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ReportService - AddAndSave ReportId: {subscription.ReportId}, UserId: {subscription.UserId} throws exception.");
            throw;
        }
    }

    /// <summary>
    /// Update the user report.
    /// </summary>
    /// <param name="subscription"></param>
    public void UpdateAndSave(UserReport subscription)
    {
        this.Context.Update(subscription);
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ReportService - UpdateAndSave ReportId: {subscription.ReportId}, UserId: {subscription.UserId} throws exception.");
            throw;
        }
    }
    #endregion
}
