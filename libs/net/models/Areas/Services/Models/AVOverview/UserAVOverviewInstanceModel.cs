using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.AVOverview;

/// <summary>
/// UserAVOverviewInstanceModel class, provides a model that represents an user report instance.
/// </summary>
public class UserAVOverviewInstanceModel : AuditColumnsModel
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
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The report status.
    /// </summary>
    public Entities.ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserAVOverviewInstanceModel.
    /// </summary>
    public UserAVOverviewInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an UserAVOverviewInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserAVOverviewInstanceModel(Entities.UserAVOverviewInstance entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.InstanceId = entity.InstanceId;
        this.SentOn = entity.SentOn;
        this.Status = entity.Status;
        this.Response = entity.Response;
        this.Email = entity.User?.Email ?? "";
        this.PreferredEmail = entity.User?.PreferredEmail ?? "";
    }

    /// <summary>
    /// Creates a new instance of an UserAVOverviewInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="instanceId"></param>
    /// <param name="sentOn"></param>
    /// <param name="status"></param>
    /// <param name="response"></param>
    public UserAVOverviewInstanceModel(int userId, long instanceId, DateTime? sentOn, Entities.ReportStatus status, JsonDocument response)
    {
        this.UserId = userId;
        this.InstanceId = instanceId;
        this.SentOn = sentOn;
        this.Status = status;
        this.Response = response;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserAVOverviewInstance(UserAVOverviewInstanceModel model)
    {
        var entity = new Entities.UserAVOverviewInstance(model.UserId, model.InstanceId, model.SentOn, model.Status, model.Response)
        {
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
