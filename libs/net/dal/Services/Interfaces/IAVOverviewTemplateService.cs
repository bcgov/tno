using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewTemplateService : IBaseService<AVOverviewTemplate, AVOverviewTemplateType>
{
    IEnumerable<AVOverviewTemplate> FindAll();
}
