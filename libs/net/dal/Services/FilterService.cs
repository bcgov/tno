using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;


public class FilterService : BaseService<Filter, int>, IFilterService
{

    #region Constructors
    public FilterService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Filter> FindAll()
    {
        return this.Context.Filters
            .AsNoTracking()
            .Include(f => f.Owner)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name)
            .ToArray();
    }

    public override Filter? FindById(int id)
    {
        return this.Context.Filters
            .Include(f => f.Owner)
            .FirstOrDefault(f => f.Id == id);
    }

    public IEnumerable<Filter> FindMyFilters(int userId)
    {
        return this.Context.Filters
            .Where(f => f.OwnerId == userId)
            .OrderBy(a => a.Name)
            .ToArray();
    }
    #endregion

}
