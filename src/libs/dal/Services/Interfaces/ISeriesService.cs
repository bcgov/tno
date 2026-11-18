using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface ISeriesService : IBaseService<Series, int>
{
    IEnumerable<Series> FindAll();
    IPaged<Series> Find(SeriesFilter filter);
    Series? Merge(int intoId, int fromId);
}
