using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Report;

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
    /// get/set - The user who is linked to the report.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - Whether this user is subscribed to the report.
    /// </summary>
    public bool IsSubscribed { get; set; }
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
        this.User = entity.User != null ? new UserModel(entity.User) : null;
        this.ReportId = entity.ReportId;
        this.IsSubscribed = entity.IsSubscribed;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserReport(UserReportModel model)
    {
        return new Entities.UserReport(model.UserId, model.ReportId)
        {
            IsSubscribed = model.IsSubscribed,
            Version = model.Version ?? 0
        };
    }
    #endregion
}
