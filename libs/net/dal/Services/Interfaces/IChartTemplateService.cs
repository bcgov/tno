using TNO.Entities;

namespace TNO.DAL.Services;

public interface IChartTemplateService : IBaseService<ChartTemplate, int>
{
    IEnumerable<ChartTemplate> FindAll();
}
