using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Variables
    private readonly ITNOElasticClient _client;
    #endregion

    #region Constructors
    public ReportService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITNOElasticClient client,
        IServiceProvider serviceProvider,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _client = client;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the reports.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Report> FindAll()
    {
        return this.Context.Reports
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
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
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(r => r.Id == id);
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
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report in the database.
    /// Update subscribers of the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override Report Update(Report entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        var originalSubscribers = this.Context.UserReports.Where(ur => ur.ReportId == entity.Id).ToArray();

        originalSubscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(s =>
        {
            var current = originalSubscribers.FirstOrDefault(rs => rs.UserId == s.UserId);
            if (current == null)
                original.SubscribersManyToMany.Add(s);
        });

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
                    original.Template.EnableSections = entity.Template.EnableSections;
                    original.Template.EnableSectionSummary = entity.Template.EnableSectionSummary;
                    original.Template.EnableSummary = entity.Template.EnableSummary;
                    original.Template.EnableCharts = entity.Template.EnableCharts;
                    original.Template.EnableChartsOverTime = entity.Template.EnableChartsOverTime;
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

        original.Name = entity.Name;
        original.Description = entity.Description;
        original.IsEnabled = entity.IsEnabled;
        original.SortOrder = entity.SortOrder;
        original.IsPublic = entity.IsPublic;
        original.TemplateId = entity.TemplateId;
        original.OwnerId = entity.OwnerId;
        original.ReportType = entity.ReportType;
        original.Filter = entity.Filter;
        original.Settings = entity.Settings;
        original.Version = entity.Version;

        return base.Update(original);
    }

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// Makes a request for each section.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(string index, Report report)
    {
        var results = new Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>();

        // Only make a request if the filter has been configured.
        var mainFilter = report.Filter.ToJson();
        if (mainFilter != "{}")
        {
            var main = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, report.Filter);
            results.Add("", main);
        }

        if (report.Settings.RootElement.TryGetProperty("sections", out JsonElement sections))
        {
            foreach (var jElement in sections.EnumerateArray())
            {
                // For each section request content based on the specified filter.
                var nameElement = jElement.GetProperty("name");
                var name = nameElement.GetString();
                if (!String.IsNullOrEmpty(name))
                {
                    var filterElement = jElement.GetProperty("filter");
                    var content = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, filterElement);
                    results.Add(name, content);
                }
            }
        }

        return results;
    }
    #endregion
}
