using System.Text.Json;

namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// UserReportModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class UserReportModel : UserModel
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
    public Entities.ReportStatus? Status { get; set; }

    /// <summary>
    /// get/set - The response from CHES for this specific subscriber.
    /// </summary>
    public JsonDocument? Response { get; set; }
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
    public UserReportModel(Entities.UserReport entity) : base(entity.User)
    {
        this.UserId = entity.UserId;
        this.ReportId = entity.ReportId;
        this.IsSubscribed = entity.IsSubscribed;
        this.Format = entity.Format;
        this.SendTo = entity.SendTo;
        this.Version = entity.Version;
    }

    /// <summary>
    /// Creates a new instance of an UserReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserReportModel(Entities.UserReport entity, Entities.User user) : base(user)
    {
        this.UserId = entity.UserId;
        this.ReportId = entity.ReportId;
        this.IsSubscribed = entity.IsSubscribed;
        this.Format = entity.Format;
        this.SendTo = entity.SendTo;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserReport(UserReportModel model)
    {
        return new Entities.UserReport(model.UserId, model.ReportId, model.IsSubscribed, model.Format, model.SendTo)
        {
            Version = model.Version ?? 0
        };
    }
    #endregion

    #region Methods
    public bool Equals(UserReportModel? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as UserReportModel);
    public override int GetHashCode() => this.Id.GetHashCode();
    #endregion
}
