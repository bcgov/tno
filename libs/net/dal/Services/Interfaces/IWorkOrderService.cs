
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

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
    /// Find all work orders for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    IEnumerable<WorkOrder> FindByContentId(long contentId);
}
