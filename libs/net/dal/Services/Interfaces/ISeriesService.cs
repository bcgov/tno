
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ISeriesService : IBaseService<Series, int>
{
    IEnumerable<Series> FindAll();
}
