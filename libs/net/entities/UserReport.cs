using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserReport class, provides a model to link users with their reports.
/// </summary>
[Table("user_report")]
public class UserReport : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the report.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report.
    /// </summary>
    [Column("report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - the report linked to the user.
    /// </summary>
    public Report? Report { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to this report.
    /// </summary>
    [Column("is_subscribed")]
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// get/set - Which distribution format the user wants to receive.
    /// </summary>
    [Column("format")]
    public ReportDistributionFormat Format { get; set; } = ReportDistributionFormat.FullText;

    /// <summary>
    /// get/set - How the email will be sent to the subscriber.
    /// </summary>
    [Column("send_to")]
    public EmailSentTo SendTo { get; set; } = EmailSentTo.To;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserReport object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="report"></param>
    /// <param name="isSubscribed"></param>
    /// <param name="format"></param>
    /// <param name="sendTo"></param>
    public UserReport(User user, Report report, bool isSubscribed = true, ReportDistributionFormat format = ReportDistributionFormat.FullText, EmailSentTo sendTo = EmailSentTo.To)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Report = report ?? throw new ArgumentNullException(nameof(report));
        this.ReportId = report.Id;
        this.IsSubscribed = isSubscribed;
        this.Format = format;
        this.SendTo = sendTo;
    }

    /// <summary>
    /// Creates a new instance of a UserReport object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reportId"></param>
    /// <param name="isSubscribed"></param>
    /// <param name="format"></param>
    /// <param name="sendTo"></param>
    public UserReport(int userId, int reportId, bool isSubscribed = true, ReportDistributionFormat format = ReportDistributionFormat.FullText, EmailSentTo sendTo = EmailSentTo.To)
    {
        this.UserId = userId;
        this.ReportId = reportId;
        this.IsSubscribed = isSubscribed;
        this.Format = format;
        this.SendTo = sendTo;
    }
    #endregion

    #region Methods
    public bool Equals(UserReport? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.ReportId == other.ReportId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserReport);
    public override int GetHashCode() => (this.UserId, this.ReportId).GetHashCode();
    #endregion
}
