using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

/// <summary>
/// SeriesService class, provides a way to perform CRUD methods on series in the database.
/// </summary>
public class SeriesService : BaseService<Series, int>, ISeriesService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SeriesService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public SeriesService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SeriesService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all series.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Series> FindAll()
    {
        return this.Context.Series
            .AsNoTracking()
            .Include(s => s.Source)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToArray();
    }

    /// <summary>
    /// Find a page of series for the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<Series> Find(SeriesFilter filter)
    {
        var query = this.Context.Series
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Series>();
        return new Paged<Series>(items, filter.Page, filter.Quantity, total);
    }

    /// <summary>
    /// Find the series for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Series? FindById(int id)
    {
        return this.Context.Series
            .Include(s => s.Source)
            .FirstOrDefault(s => s.Id == id);
    }
    #endregion
}
