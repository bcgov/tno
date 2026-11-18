using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserAVOverviewModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class UserAVOverviewModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report.
    /// </summary>
    public Entities.AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to the av evening overview report.
    /// </summary>
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// get/set - Which distribution format the user wants to receive.
    /// </summary>
    public Entities.ReportDistributionFormat Format { get; set; } = Entities.ReportDistributionFormat.FullText;

    /// <summary>
    /// get/set - How the email will be sent to the subscriber.
    /// </summary>
    public Entities.EmailSentTo SendTo { get; set; }

    /// <summary>
    /// get/set - The status of the report for this specific subscriber.
    /// </summary>
    public Entities.ReportStatus? LinkStatus { get; set; }

    /// <summary>
    /// get/set - The response from CHES for this specific subscriber.
    /// </summary>
    public JsonDocument? LinkResponse { get; set; }

    /// <summary>
    /// get/set - The status of the report for this specific subscriber.
    /// </summary>
    public Entities.ReportStatus? TextStatus { get; set; }

    /// <summary>
    /// get/set - The response from CHES for this specific subscriber.
    /// </summary>
    public JsonDocument? TextResponse { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserAVOverviewModel.
    /// </summary>
    public UserAVOverviewModel() { }

    /// <summary>
    /// Creates a new instance of an UserAVOverviewModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserAVOverviewModel(Entities.UserAVOverview entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.TemplateType = entity.TemplateType;
        this.IsSubscribed = entity.IsSubscribed;
        this.SendTo = entity.SendTo;
    }
    #endregion
}
