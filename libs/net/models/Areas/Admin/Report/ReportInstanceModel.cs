using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// ReportInstanceModel class, provides a model that represents an report instance.
/// </summary>
public class ReportInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report definition.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the owner of this report instance.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of this report instance.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportInstanceModel.
    /// </summary>
    public ReportInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an ReportInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportInstanceModel(Entities.ReportInstance entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.ReportId = entity.ReportId;
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.PublishedOn = entity.PublishedOn;
        this.Response = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Response, options) ?? new Dictionary<string, object>();
    }
    #endregion
}
