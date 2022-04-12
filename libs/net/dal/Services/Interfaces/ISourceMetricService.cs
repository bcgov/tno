
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ISourceMetricService : IBaseService<SourceMetric, int>
{
    IEnumerable<SourceMetric> FindAll();
}
