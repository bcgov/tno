using TNO.Entities;

namespace TNO.DAL.Services;

public interface IChartTemplateService : IBaseService<ChartTemplate, int>
{
    /// <summary>
    /// Find all chart templates.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ChartTemplate> FindAll();

    /// <summary>
    /// Determine if this chart template is being used by any reports.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool IsInUse(int id);
}
