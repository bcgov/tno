using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportTemplateService : IBaseService<ReportTemplate, int>
{
    /// <summary>
    /// Return all the report templates.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ReportTemplate> FindAll();

    /// <summary>
    /// Find all the public report templates.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ReportTemplate> FindPublic();

    /// <summary>
    /// Determine if this report template is being used by any reports.
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    bool IsInUse(int templateId);
}
