
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IMetricService : IBaseService<Metric, int>
{
    IEnumerable<Metric> FindAll();
}
