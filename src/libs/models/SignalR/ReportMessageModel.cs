using TNO.Entities;

namespace TNO.API.Models.SignalR;

/// <summary>
/// ReportMessageModel class, provides a model that represents a report.
/// </summary>
public class ReportMessageModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Primary key
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - A subject to identify the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The status of the report.
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - A message to identify what action was taken.
    /// </summary>
    public string? Message { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportMessageModel.
    /// </summary>
    public ReportMessageModel() { }

    /// <summary>
    /// Creates a new instance of an ReportMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportMessageModel(ReportInstance entity) : base(entity)
    {
        this.Id = entity.Id;
        this.ReportId = entity.ReportId;
        this.Subject = entity.Subject;
        this.Status = entity.Status;
        this.OwnerId = entity.OwnerId;
    }
    #endregion
}
