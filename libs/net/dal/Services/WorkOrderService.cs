using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// WorkOrderService class, provides the Data Access Layer (DAL) service for interacting with work order with the database.
/// </summary>
public class WorkOrderService : BaseService<WorkOrder, long>, IWorkOrderService
{
    #region Properties
    #endregion
    /// <summary>
    /// Creates a new instance of a WorkOrderService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    #region Constructors
    public WorkOrderService(TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<WorkOrderService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find work orders that match the specified 'filter'.
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of work order items that match the filter.</returns>
    public IPaged<WorkOrder> Find(WorkOrderFilter filter)
    {
        var query = this.Context.WorkOrders
            .Include(m => m.Content)
            .AsNoTracking()
            .AsQueryable();

        if (filter.WorkType.HasValue)
            query = query.Where(c => c.WorkType == filter.WorkType);
        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status);

        if (filter.ContentId.HasValue)
            query = query.Where(c => c.ContentId == filter.ContentId);
        if (filter.RequestorId.HasValue)
            query = query.Where(c => c.RequestorId == filter.RequestorId);
        if (filter.AssignedId.HasValue)
            query = query.Where(c => c.AssignedId == filter.AssignedId);

        if (filter.CreatedOn.HasValue)
            query = query.Where(c => c.CreatedOn == filter.CreatedOn.Value.ToUniversalTime());
        if (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime() && c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());
        else if (filter.CreatedStartOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime());
        else if (filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());

        if (filter.UpdatedOn.HasValue)
            query = query.Where(c => c.UpdatedOn == filter.UpdatedOn.Value.ToUniversalTime());
        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime() && c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());
        else if (filter.UpdatedStartOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime());
        else if (filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());

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
            query = query.OrderByDescending(c => c.CreatedOn);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<WorkOrder>();
        return new Paged<WorkOrder>(items, filter.Page, filter.Quantity, total);
    }

    /// <summary>
    /// Find all work orders for the specified 'contentId'.
    /// Generally speaking there shouldn't be many work order requests for a single content item,
    /// which means it should be quicker to return the lot and filter afterwards.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<WorkOrder> FindByContentId(long contentId)
    {
        var query = this.Context.WorkOrders
            .AsNoTracking()
            .Where(c => c.ContentId == contentId)
            .OrderByDescending(c => c.CreatedOn);

        return query.ToArray();
    }

    /// <summary>
    /// Find the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override WorkOrder? FindById(long id)
    {
        return this.Context.WorkOrders
            .Include(m => m.Content)
            .FirstOrDefault(m => m.Id == id);
    }
    #endregion
}
