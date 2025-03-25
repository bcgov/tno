using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.WorkOrder;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

/// <summary>
/// WorkOrderService class, provides the Data Access Layer (DAL) service for interacting with work order with the database.
/// </summary>
public class WorkOrderService : BaseService<WorkOrder, long>, IWorkOrderService
{
    #region Variables
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    #endregion
    /// <summary>
    /// Creates a new instance of a WorkOrderService object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    #region Constructors
    public WorkOrderService(
        IOptions<JsonSerializerOptions> serializerOptions,
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<WorkOrderService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _serializerOptions = serializerOptions.Value;
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
            .AsNoTracking()
            .Include(w => w.Requestor)
            .AsQueryable();

        if (filter.WorkType.HasValue)
            query = query.Where(c => c.WorkType == filter.WorkType);
        if (filter.Status?.Any() == true)
            query = query.Where(c => filter.Status.Contains(c.Status));

        if (filter.ContentId.HasValue)
            query = query.Where(c => c.ContentId == filter.ContentId || EF.Functions.JsonContains(c.Configuration, $"{{\"contentId\":{filter.ContentId}}}"));
        if (filter.RequestorId.HasValue)
            query = query.Where(c => c.RequestorId == filter.RequestorId);
        if (filter.AssignedId.HasValue)
            query = query.Where(c => c.AssignedId == filter.AssignedId);
        if (filter.Keywords != null)
            query = query.Where(c => EF.Functions.Like(c.Content!.Headline.ToLower(), $"%{filter.Keywords.ToLower()}%") ||
                EF.Functions.Like(c.Requestor!.Username.ToLower(), $"%{filter.Keywords.ToLower()}%"));
        if (filter.IsApproved.HasValue)
            query = query.Where(c => c.Content!.IsApproved == filter.IsApproved.Value);

        if (filter.SourceIds.Any())
            query = query.Where(c => filter.SourceIds.Contains(c.Content!.SourceId ?? 0));
        if (filter.SeriesIds.Any())
            query = query.Where(c => filter.SeriesIds.Contains(c.Content!.SeriesId ?? 0));
        if (filter.MediaTypeIds.Any())
            query = query.Where(c => filter.MediaTypeIds.Contains(c.Content!.MediaTypeId));

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

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 50;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<WorkOrder>();

        return new Paged<WorkOrder>(items, page, quantity, total);
    }

    /// <summary>
    /// Find work orders that match the specified 'filter' and only return the latest distinct record for each content item..
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of work order items that match the filter.</returns>
    public IPaged<WorkOrderModel> FindDistinctWorkOrders(WorkOrderFilter filter)
    {
        var query =
        from t in
            from w in this.Context.WorkOrders
            join w2 in
                from wo in this.Context.WorkOrders
                group wo by wo.ContentId into g
                select new
                {
                    ContentId = g.Key,
                    LatestCreatedOn = g.Max(wo => wo.CreatedOn)
                }
            on new { w.ContentId, w.CreatedOn }
            equals new { w2.ContentId, CreatedOn = w2.LatestCreatedOn }
            select new
            {
                w.Id,
                w.AssignedId,
                w.Configuration,
                w.ContentId,
                w.CreatedBy,
                w.CreatedOn,
                w.Description,
                w.Note,
                w.RequestorId,
                w.Status,
                w.UpdatedBy,
                w.UpdatedOn,
                w.Version,
                w.WorkType
            }
        join u in this.Context.Users on t.RequestorId equals u.Id into requestorJoin
        from u in requestorJoin.DefaultIfEmpty()
        join u0 in this.Context.Users on t.AssignedId equals u0.Id into assignedJoin
        from u0 in assignedJoin.DefaultIfEmpty()
        join c in this.Context.Contents on t.ContentId equals c.Id into contentJoin
        from c in contentJoin.DefaultIfEmpty()
        join m in this.Context.MediaTypes on c.MediaTypeId equals m.Id into mediaTypeJoin
        from m in mediaTypeJoin.DefaultIfEmpty()
        join s in this.Context.Series on c.SeriesId equals s.Id into seriesJoin
        from s in seriesJoin.DefaultIfEmpty()
        join s0 in this.Context.Sources on c.SourceId equals s0.Id into sourceJoin
        from s0 in sourceJoin.DefaultIfEmpty()
        orderby t.CreatedOn descending
        select new WorkOrderModel
        {
            Id = t.Id,
            WorkType = t.WorkType,
            Status = t.Status,
            Description = t.Description,
            Note = t.Note,
            Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(t.Configuration, _serializerOptions) ?? new Dictionary<string, object>(),
            RequestorId = t.RequestorId,
            Requestor = t.RequestorId != null ? new UserModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                DisplayName = u.DisplayName,
                FirstName = u.FirstName,
                LastName = u.LastName
            } : null,
            AssignedId = t.AssignedId,
            Assigned = t.AssignedId != null ? new UserModel
            {
                Id = u0.Id,
                Username = u0.Username,
                Email = u0.Email,
                DisplayName = u0.DisplayName,
                FirstName = u0.FirstName,
                LastName = u0.LastName
            } : null,
            ContentId = t.ContentId,
            Content = t.ContentId != null ? new ContentModel
            {
                Id = c.Id,
                Headline = c.Headline,
                OtherSource = c.OtherSource,
                IsApproved = c.IsApproved,
                MediaType = m.Name,
                Series = s.Name,
                SeriesId = s.Id,
                SourceId = s0.Id,
                MediaTypeId = m.Id,
            } : null,
            CreatedOn = t.CreatedOn,
            CreatedBy = t.CreatedBy,
            UpdatedOn = t.UpdatedOn,
            UpdatedBy = t.UpdatedBy,
            Version = t.Version
        };

        if (filter.WorkType.HasValue)
            query = query.Where(c => c.WorkType == filter.WorkType);
        if (filter.Status?.Any() == true)
            query = query.Where(c => filter.Status.Contains(c.Status));

        if (filter.ContentId.HasValue)
            query = query.Where(c => c.ContentId == filter.ContentId);
        if (filter.RequestorId.HasValue)
            query = query.Where(c => c.RequestorId == filter.RequestorId);
        if (filter.AssignedId.HasValue)
            query = query.Where(c => c.AssignedId == filter.AssignedId);
        if (filter.Keywords != null)
            query = query.Where(c => EF.Functions.Like(c.Content!.Headline.ToLower(), $"%{filter.Keywords.ToLower()}%") ||
                EF.Functions.Like(c.Content!.OtherSource.ToLower(), $"{filter.Keywords.ToLower()}") ||
                EF.Functions.Like(c.Requestor!.Username.ToLower(), $"%{filter.Keywords.ToLower()}%") ||
                EF.Functions.Like(c.Assigned!.Username.ToLower(), $"%{filter.Keywords.ToLower()}%"));
        if (filter.IsApproved.HasValue)
            query = query.Where(c => c.Content!.IsApproved == filter.IsApproved.Value);

        if (filter.SourceIds.Any())
            query = query.Where(c => filter.SourceIds.Contains(c.Content!.SourceId ?? 0));
        if (filter.SeriesIds.Any())
            query = query.Where(c => filter.SeriesIds.Contains(c.Content!.SeriesId ?? 0));
        if (filter.MediaTypeIds.Any())
            query = query.Where(c => filter.MediaTypeIds.Contains(c.Content!.MediaTypeId ?? 0));

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

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 50;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<WorkOrderModel>();

        return new Paged<WorkOrderModel>(items, page, quantity, total);
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
    /// Find all work orders for the specified 'locationId' and 'path'.
    /// Generally speaking there shouldn't be many work order requests for a single file,
    /// which means it should be quicker to return the lot and filter afterwards.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerable<WorkOrder> FindByFile(int locationId, string path)
    {
        var query = this.Context.WorkOrders
            .AsNoTracking()
            .Where(c => EF.Functions.JsonContains(c.Configuration, $"{{\"locationId\":{locationId}, \"path\":\"{path}\"}}"))
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
            .FirstOrDefault(m => m.Id == id);
    }
    #endregion
}
