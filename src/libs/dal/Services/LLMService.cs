using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Admin.Models.LLM;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class LLMService : BaseService<Entities.LLM, int>, ILLMService
{
    #region Variables
    private const string UsageTypeReportSection = "ReportSection";
    private const string UsageTypeAutomationProfile = "AutomationProfile";
    private const string UsageTypeContentAnalysis = "ContentAnalysis";
    private const string UsageTypeSearch = "Search";
    #endregion

    #region Properties
    #endregion

    #region Constructors
    public LLMService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<LLMService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.LLM> FindAll()
    {
        return this.Context.LLM
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public Entities.LLM? FindByName(string name)
    {
        return this.Context.LLM
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }

    public override Entities.LLM? FindById(int id)
    {
        return this.Context.LLM
            .FirstOrDefault(c => c.Id == id);
    }

    /// <inheritdoc/>
    public LLMUsageModel FindUsage(int id)
    {
        var items = new List<LLMUsageItemModel>();

        foreach (var section in FindAISections(id, false))
        {
            var label = section.Settings.RootElement.TryGetProperty("label", out var value) && value.ValueKind == System.Text.Json.JsonValueKind.String ? value.GetString() : null;
            items.Add(new LLMUsageItemModel
            {
                Type = UsageTypeReportSection,
                Id = section.ReportId,
                SectionId = section.Id,
                Name = section.Report?.Name ?? "",
                Detail = String.IsNullOrWhiteSpace(label) ? section.Name : label,
                Owner = section.Report?.Owner?.Username,
                IsEnabled = section.IsEnabled && section.Report?.IsEnabled == true,
            });
        }

        foreach (var profile in this.Context.AutomationProfiles.AsNoTracking().OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToArray())
        {
            var references = LLMUsageJson.FindDefinitionReferences(profile.Definition, id).ToList();
            if (profile.LLMId == id) references.Insert(0, "Profile default");
            if (references.Count == 0) continue;
            items.Add(new LLMUsageItemModel
            {
                Type = UsageTypeAutomationProfile,
                Id = profile.Id,
                Name = profile.Name,
                Detail = String.Join(", ", references),
                IsEnabled = profile.IsEnabled,
            });
        }

        foreach (var setting in FindSettings(id, false))
        {
            var isContentAnalysis = setting.Name == AdminConfigurableSettingNames.ContentAnalysisLLMId.ToString();
            items.Add(new LLMUsageItemModel
            {
                Type = isContentAnalysis ? UsageTypeContentAnalysis : UsageTypeSearch,
                Name = isContentAnalysis ? "Content Analysis" : "Advanced search AI prompts",
                Detail = isContentAnalysis ? "Analyzes stories" : "Generates search filters",
            });
        }

        return new LLMUsageModel
        {
            LLMId = id,
            Items = items,
            ContentAnalysisResultCount = this.Context.ContentAnalyses.LongCount(a => a.LLMId == id),
        };
    }

    /// <inheritdoc/>
    public LLMMigrationCheckModel CheckMigration(int id, int targetId, bool hasDefaultApiKey, bool hasDefaultDeployment)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var target = this.Context.LLM.AsNoTracking().FirstOrDefault(l => l.Id == targetId);
        if (target == null)
            return new LLMMigrationCheckModel { Errors = new[] { "The target connection does not exist." } };

        var items = FindUsage(id).Items.ToArray();
        if (items.Length == 0)
            return new LLMMigrationCheckModel { Errors = new[] { "Nothing uses this connection." } };

        var hasReports = items.Any(i => i.Type == UsageTypeReportSection);
        var hasSearch = items.Any(i => i.Type == UsageTypeSearch);
        var hasContentAnalysis = items.Any(i => i.Type == UsageTypeContentAnalysis);
        var hasAutomation = items.Any(i => i.Type == UsageTypeAutomationProfile);
        var hasApiKey = !String.IsNullOrWhiteSpace(target.ApiKey);
        var hasDeployment = !String.IsNullOrWhiteSpace(target.DeploymentName);

        if (targetId == id) errors.Add("Choose a different connection.");
        if (!target.IsEnabled) errors.Add($"'{target.Name}' is disabled.");
        if (!String.IsNullOrWhiteSpace(target.AgentName)) errors.Add($"'{target.Name}' is an agent. Migrations need a deployment that is called directly.");
        if (target.ProjectEndpoint == null) errors.Add($"'{target.Name}' has no project endpoint.");

        // Content Analysis and Automation only read the connection. Reports and search also have configured defaults.
        if (!hasApiKey && (hasContentAnalysis || hasAutomation || !hasDefaultApiKey))
            errors.Add($"'{target.Name}' has no API key, which {DescribeConsumers(hasContentAnalysis, hasAutomation, hasReports, hasSearch, hasDefaultApiKey)} needs.");
        if (!hasDeployment && (hasContentAnalysis || hasAutomation || !hasDefaultDeployment))
            errors.Add($"'{target.Name}' has no deployment name, which {DescribeConsumers(hasContentAnalysis, hasAutomation, hasReports, hasSearch, hasDefaultDeployment)} needs.");

        var hasValidLimits = target.ContextWindow > 0 && target.MaxOutputTokens > 0 && target.MaxOutputTokens < target.ContextWindow;
        if ((hasReports || hasContentAnalysis) && !hasValidLimits)
            errors.Add($"'{target.Name}' needs a context window and a maximum output less than it, for Report AI sections and Content Analysis.");

        var sections = FindAISections(id, false);
        var nonAdminReports = sections.Where(s => s.Report?.Owner?.Roles.Contains("[administrator]", StringComparison.OrdinalIgnoreCase) != true).Select(s => s.ReportId).Distinct().Count();
        if (!target.IsPublic && nonAdminReports > 0)
            warnings.Add($"'{target.Name}' is not public, and {nonAdminReports} report(s) moving to it are not owned by an administrator. Their owners cannot choose it again if they change model.");

        var temperatures = sections.Count(s => LLMUsageJson.GetClampedTemperature(s.Settings, target.MinTemperature, target.MaxTemperature).HasValue);
        if (temperatures > 0)
            warnings.Add($"{temperatures} report section(s) will have their temperature moved into the range '{target.Name}' allows.");

        return new LLMMigrationCheckModel { Errors = errors, Warnings = warnings };
    }

    /// <inheritdoc/>
    public LLMMigrationResultModel Migrate(int id, int targetId, bool hasDefaultApiKey, bool hasDefaultDeployment)
    {
        var check = CheckMigration(id, targetId, hasDefaultApiKey, hasDefaultDeployment);
        if (check.Errors.Any()) throw new BadRequestException(String.Join(" ", check.Errors));
        var target = this.Context.LLM.AsNoTracking().First(l => l.Id == targetId);

        var migrated = 0;
        var temperaturesChanged = 0;
        foreach (var section in FindAISections(id, true))
        {
            var temperature = LLMUsageJson.GetClampedTemperature(section.Settings, target.MinTemperature, target.MaxTemperature);
            if (temperature.HasValue) temperaturesChanged++;
            section.Settings = LLMUsageJson.ReplaceSectionLLM(section.Settings, targetId, temperature);
            migrated++;
        }

        foreach (var profile in this.Context.AutomationProfiles.ToArray())
        {
            var definition = LLMUsageJson.ReplaceDefinitionReferences(profile.Definition, id, targetId);
            var usesDefault = profile.LLMId == id;
            if (definition == null && !usesDefault) continue;
            if (definition != null) profile.Definition = definition;
            if (usesDefault) profile.LLMId = targetId;
            migrated++;
        }

        foreach (var setting in FindSettings(id, true))
        {
            setting.Value = targetId.ToString();
            migrated++;
        }

        this.Context.UpdateCache<ReportSection>();
        this.Context.UpdateCache<Report>();
        this.Context.UpdateCache<AutomationProfile>();
        this.Context.UpdateCache<Setting>();
        this.Context.CommitTransaction();

        return new LLMMigrationResultModel { Migrated = migrated, TemperaturesChanged = temperaturesChanged, Usage = FindUsage(id) };
    }

    /// <summary>
    /// Report AI sections keep the connection in their JSON settings, so they are compared here.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="track"></param>
    /// <returns></returns>
    private ReportSection[] FindAISections(int id, bool track)
    {
        var query = this.Context.ReportSections
            .Include(s => s.Report).ThenInclude(r => r!.Owner)
            .Where(s => s.SectionType == ReportSectionType.AI);
        return (track ? query : query.AsNoTracking()).AsEnumerable()
            .Where(s => LLMUsageJson.GetSectionLLMId(s.Settings) == id)
            .OrderBy(s => s.Report?.Name).ThenBy(s => s.SortOrder)
            .ToArray();
    }

    /// <summary>
    /// Find the global settings that choose the specified connection.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="track"></param>
    /// <returns></returns>
    private Setting[] FindSettings(int id, bool track)
    {
        var names = new[] { AdminConfigurableSettingNames.ContentAnalysisLLMId.ToString(), ILLMService.FilterAISettingName };
        var query = this.Context.Settings.Where(s => names.Contains(s.Name));
        return (track ? query : query.AsNoTracking()).AsEnumerable()
            .Where(s => int.TryParse(s.Value, out var value) && value == id)
            .OrderBy(s => s.Name)
            .ToArray();
    }

    /// <summary>
    /// Name what needs a value, leaving out reports and search when they can use a configured default.
    /// </summary>
    private static string DescribeConsumers(bool contentAnalysis, bool automation, bool reports, bool search, bool hasDefault)
    {
        var names = new List<string>();
        if (contentAnalysis) names.Add("Content Analysis");
        if (automation) names.Add("Automation");
        if (!hasDefault && reports) names.Add("Report AI");
        if (!hasDefault && search) names.Add("Search");
        return String.Join(" and ", names);
    }

    public IPaged<Entities.LLM> Find(LLMFilter filter)
    {
        var query = this.Context.LLM
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));
        if (filter.IsPublic.HasValue)
            query = query.Where(c => c.IsPublic == filter.IsPublic);

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<Entities.LLM>();
        return new Paged<Entities.LLM>(items, page, quantity, total);
    }

    /// <summary>
    /// Add the specified data aiModel to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Entities.LLM AddAndSave(Entities.LLM entity)
    {
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data aiModel in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Entities.LLM UpdateAndSave(Entities.LLM entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
        return base.UpdateAndSave(entity);
    }
    #endregion
}
