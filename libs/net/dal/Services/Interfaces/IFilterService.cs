using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFilterService : IBaseService<Filter, int>
{
    IEnumerable<Filter> FindAll();
    IEnumerable<Filter> FindMyFilters(int userId);
}
