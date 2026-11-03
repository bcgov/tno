using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IFilterService : IBaseService<Filter, int>
{
    IEnumerable<Filter> Find(FilterFilter? filter = null);

    /// <summary>
    /// Find a page of filters for the specified filter, with the total that match.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IPaged<Filter> FindPage(FilterFilter filter);
}
