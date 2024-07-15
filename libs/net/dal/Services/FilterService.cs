using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;


public class FilterService : BaseService<Filter, int>, IFilterService
{

    #region Constructors
    public FilterService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all filters for the specified filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Filter> Find(FilterFilter? filter = null)
    {
        var query = this.Context.Filters
            .AsNoTracking()
            .Include(f => f.Owner)
            .AsQueryable();

        var predicate = PredicateBuilder.New<Filter>(true);

        if (!String.IsNullOrWhiteSpace(filter?.Name))
            predicate = predicate.And(c => EF.Functions.Like(c.Name.ToLower(), $"{filter.Name.ToLower()}%"));
        if (filter?.OwnerId.HasValue == true)
            predicate = predicate.And(p => p.OwnerId == filter.OwnerId);

        query = query.Where(predicate);

        if (filter?.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(u => u.Name);

        if (filter != null && filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

        return query.ToArray();
    }

    public override Filter? FindById(int id)
    {
        return this.Context.Filters
            .Include(f => f.Owner)
            .FirstOrDefault(f => f.Id == id);
    }
    #endregion

}
