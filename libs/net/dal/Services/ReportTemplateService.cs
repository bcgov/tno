using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportTemplateService : BaseService<ReportTemplate, int>, IReportTemplateService
{
    #region Variables
    #endregion

    #region Constructors
    public ReportTemplateService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ReportTemplateService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the report templates.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ReportTemplate> FindAll()
    {
        return this.Context.ReportTemplates
            .AsNoTracking()
            .Include(r => r.ChartTemplatesManyToMany).ThenInclude(s => s.ChartTemplate)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find the report template for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override ReportTemplate? FindById(int id)
    {
        return this.Context.ReportTemplates
            .Include(r => r.ChartTemplatesManyToMany).ThenInclude(s => s.ChartTemplate)
            .FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Add the new report template to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ReportTemplate Add(ReportTemplate entity)
    {
        this.Context.AddRange(entity.ChartTemplatesManyToMany);
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report template in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override ReportTemplate Update(ReportTemplate entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        var charts = this.Context.ReportTemplateChartTemplates.Where(ur => ur.ReportTemplateId == entity.Id).ToArray();

        charts.Except(entity.ChartTemplatesManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.ChartTemplatesManyToMany.ForEach(s =>
        {
            var current = charts.FirstOrDefault(rs => rs.ChartTemplateId == s.ChartTemplateId);
            if (current == null)
                original.ChartTemplatesManyToMany.Add(s);
        });

        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);

        return base.Update(original);
    }
    #endregion
}
