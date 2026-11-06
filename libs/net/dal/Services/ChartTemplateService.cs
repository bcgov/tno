using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Admin.Models.ChartTemplate;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ChartTemplateService : BaseService<ChartTemplate, int>, IChartTemplateService
{
    #region Variables
    #endregion

    #region Constructors
    public ChartTemplateService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ChartTemplateService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the chart templates.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ChartTemplate> FindAll()
    {
        return this.Context.ChartTemplates
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Determine if this chart template is being used by any reports.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsInUse(int id)
    {
        return this.Context.ReportTemplateChartTemplates.Any(rt => rt.ChartTemplateId == id) || this.Context.ReportSectionChartTemplates.Any(rs => rs.ChartTemplateId == id);
    }

    /// <summary>
    /// Find the reports that have a section using this chart template.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<ChartTemplateUsageModel> FindUsage(int id)
    {
        return this.Context.ReportSectionChartTemplates
            .AsNoTracking()
            .Include(c => c.ReportSection).ThenInclude(s => s!.Report).ThenInclude(r => r!.Owner)
            .Where(c => c.ChartTemplateId == id)
            .AsEnumerable()
            .Select(c => c.ReportSection!)
            .GroupBy(s => s.ReportId)
            .Select(group =>
            {
                var report = group.First().Report!;
                return new ChartTemplateUsageModel
                {
                    Id = report.Id,
                    Name = report.Name,
                    Sections = group.OrderBy(s => s.SortOrder).Select(GetSectionLabel).ToArray(),
                    Owner = report.Owner?.Username,
                    IsEnabled = report.IsEnabled,
                };
            })
            .OrderBy(r => r.Name)
            .ToArray();
    }

    /// <summary>
    /// A section's name is generated, so people know it by its label.
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    private static string GetSectionLabel(ReportSection section)
    {
        var label = section.Settings.RootElement.TryGetProperty("label", out var value) && value.ValueKind == System.Text.Json.JsonValueKind.String ? value.GetString() : null;
        return String.IsNullOrWhiteSpace(label) ? section.Name : label;
    }
    #endregion
}
