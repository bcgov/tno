
using System.Text.Json;
using TNO.API.Areas.Editor.Models.WorkOrder;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IWorkOrderService : IBaseService<WorkOrder, long>
{
    /// <summary>
    /// Find a page of work orders that match the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IPaged<WorkOrder> Find(WorkOrderFilter filter);

    /// <summary>
    /// Find work orders that match the specified 'filter' and only return the latest distinct record for each content item..
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of work order items that match the filter.</returns>
    IPaged<WorkOrderModel> FindDistinctWorkOrders(WorkOrderFilter filter);

    /// <summary>
    /// Find all work orders for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    IEnumerable<WorkOrder> FindByContentId(long contentId);

    /// <summary>
    /// Find all work orders for the specified 'locationId' and 'path'.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerable<WorkOrder> FindByFile(int locationId, string path);
}
