using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserReportModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class UserReportModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - the report.
    /// </summary>
    public ReportModel? Report { get; set; }

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
    /// Creates a new instance of an UserReportModel.
    /// </summary>
    public UserReportModel() { }

    /// <summary>
    /// Creates a new instance of an UserReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserReportModel(Entities.UserReport entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.ReportId = entity.ReportId;
        this.Report = entity.Report != null ? new ReportModel(entity.Report) : null;
        this.IsSubscribed = entity.IsSubscribed;
        this.Format = entity.Format;
        this.SendTo = entity.SendTo;
    }
    #endregion
}
