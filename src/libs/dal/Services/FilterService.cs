using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
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
        var query = Sort(Query(filter), filter);

        if (filter != null && filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

        return query.ToArray();
    }

    /// <summary>
    /// Find a page of filters for the specified filter, with the total that match.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<Filter> FindPage(FilterFilter filter)
    {
        var query = Query(filter);
        var total = query.Count();

        var page = Math.Max(1, filter.Page ?? 1);
        var quantity = Math.Clamp(filter.Quantity ?? 50, 1, 500);
        var items = Sort(query, filter)
            .Skip((page - 1) * quantity)
            .Take(quantity)
            .ToArray();
        return new Paged<Filter>(items, page, quantity, total);
    }

    /// <summary>
    /// The filters that match the specified filter, with their owner.
    /// </summary>
    private IQueryable<Filter> Query(FilterFilter? filter)
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
        if (!String.IsNullOrWhiteSpace(filter?.Keyword))
        {
            var term = $"%{EscapeLike(filter.Keyword.Trim())}%";
            predicate = predicate.And(f => EF.Functions.ILike(f.Name, term, LikeEscape)
                || EF.Functions.ILike(f.Description, term, LikeEscape)
                || (f.Owner != null && (EF.Functions.ILike(f.Owner.Username, term, LikeEscape)
                    || EF.Functions.ILike(f.Owner.DisplayName, term, LikeEscape)
                    || EF.Functions.ILike(f.Owner.FirstName, term, LikeEscape)
                    || EF.Functions.ILike(f.Owner.LastName, term, LikeEscape))));
        }

        return query.Where(predicate);
    }

    /// <summary>
    /// Order the filters by the specified sort, or by name.
    /// </summary>
    private static IQueryable<Filter> Sort(IQueryable<Filter> query, FilterFilter? filter)
    {
        if (filter?.Sort?.Any() != true) return query.OrderBy(u => u.Name);

        query = query.OrderByProperty(filter.Sort.First());
        foreach (var sort in filter.Sort.Skip(1))
        {
            query = query.ThenByProperty(sort);
        }
        return query;
    }

    /// <summary>The escape character for ILIKE patterns; Npgsql otherwise sends ESCAPE '', which disables escaping.</summary>
    private const string LikeEscape = "\\";

    /// <summary>
    /// Escape the LIKE wildcards in text, so it is matched literally.
    /// </summary>
    private static string EscapeLike(string text)
        => text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public override Filter? FindById(int id)
    {
        return this.Context.Filters
            .Include(f => f.Owner)
            .FirstOrDefault(f => f.Id == id);
    }
    #endregion

}
