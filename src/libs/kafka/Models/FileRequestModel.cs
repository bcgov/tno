using System.Text.Json;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// FileRequestModel class, provides a model for requesting a remote file.
/// </summary>
public class FileRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The location Id where the file resides.
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// get/set - The path to the file.
    /// </summary>
    public string Path { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FileRequestModel object.
    /// </summary>
    public FileRequestModel() : base(WorkOrderType.FileRequest) { }

    /// <summary>
    /// Creates a new instance of an FileRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    public FileRequestModel(long workOrderId, int locationId, string path, int? requestorId, string requestor) : base(workOrderId, WorkOrderType.FileRequest, requestorId, requestor, DateTime.UtcNow)
    {
        this.LocationId = locationId;
        this.Path = path;
    }

    /// <summary>
    /// Creates a new instance of an FileRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public FileRequestModel(WorkOrder workOrder) : base(workOrder.Id, workOrder.WorkType, workOrder.RequestorId, workOrder.Requestor?.DisplayName ?? "", workOrder.CreatedOn)
    {
        if (workOrder.Configuration.RootElement.TryGetProperty("locationId", out JsonElement elementLocation) && elementLocation.TryGetInt32(out int locationId))
        {
            this.LocationId = locationId;
        }
        else throw new ArgumentException("Work order must be for a file request and contain 'locationId' property.");

        if (workOrder.Configuration.RootElement.TryGetProperty("path", out JsonElement elementPath))
        {
            this.Path = elementPath.GetString() ?? throw new ArgumentException("Work order 'path' configuration property cannot be empty or null.");
        }
        else throw new ArgumentException("Work order must be for a file request and contain 'path' property.");
    }
    #endregion
}
