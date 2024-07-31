using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// UserReportInstanceModel class, provides a model that represents an user report instance.
/// </summary>
public class UserReportInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the report instance.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the owner of the instance.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - user's email.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// get/set - User's preferred email.
    /// </summary>
    public string PreferredEmail { get; set; } = "";

    /// <summary>
    /// get/set - The date and time the report was sent on.
    /// </summary>
    public DateTime? LinkSentOn { get; set; }

    /// <summary>
    /// get/set - The report status.
    /// </summary>
    public Entities.ReportStatus LinkStatus { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public JsonDocument LinkResponse { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The date and time the report was sent on.
    /// </summary>
    public DateTime? TextSentOn { get; set; }

    /// <summary>
    /// get/set - The report status.
    /// </summary>
    public Entities.ReportStatus TextStatus { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public JsonDocument TextResponse { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserReportInstanceModel.
    /// </summary>
    public UserReportInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an UserReportInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserReportInstanceModel(Entities.UserReportInstance entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.InstanceId = entity.InstanceId;
        this.LinkSentOn = entity.LinkSentOn;
        this.LinkStatus = entity.LinkStatus;
        this.LinkResponse = entity.LinkResponse;
        this.TextSentOn = entity.TextSentOn;
        this.TextStatus = entity.TextStatus;
        this.TextResponse = entity.TextResponse;
        this.Email = entity.User?.Email ?? "";
        this.PreferredEmail = entity.User?.PreferredEmail ?? "";
    }

    /// <summary>
    /// Creates a new instance of an UserReportInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="instanceId"></param>
    /// <param name="sentOn"></param>
    /// <param name="status"></param>
    /// <param name="response"></param>
    public UserReportInstanceModel(int userId, long instanceId)
    {
        this.UserId = userId;
        this.InstanceId = instanceId;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserReportInstance(UserReportInstanceModel model)
    {
        var entity = new Entities.UserReportInstance(model.UserId, model.InstanceId)
        {
            LinkStatus = model.LinkStatus,
            LinkSentOn = model.LinkSentOn,
            LinkResponse = model.LinkResponse,
            TextStatus = model.TextStatus,
            TextSentOn = model.TextSentOn,
            TextResponse = model.TextResponse,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
