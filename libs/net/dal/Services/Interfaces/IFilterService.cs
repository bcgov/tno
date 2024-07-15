using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IFilterService : IBaseService<Filter, int>
{
    IEnumerable<Filter> Find(FilterFilter? filter = null);
}
