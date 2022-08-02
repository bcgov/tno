using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface ISeriesService : IBaseService<Series, int>
{
    IEnumerable<Series> FindAll();
    IPaged<Series> Find(SeriesFilter filter);
}
