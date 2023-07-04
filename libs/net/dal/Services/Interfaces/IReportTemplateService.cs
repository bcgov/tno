using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportTemplateService : IBaseService<ReportTemplate, int>
{
    IEnumerable<ReportTemplate> FindAll();
}
