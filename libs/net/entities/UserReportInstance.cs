using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// UserReport class, provides a model to link users with their report instances to track email status.
/// </summary>
[Table("user_report_instance")]
public class UserReportInstance : AuditColumns
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
    [Column("report_instance_id")]
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - the report linked to the user.
    /// </summary>
    public ReportInstance? Instance { get; set; }

    /// <summary>
    /// get/set - The status of this report.
    /// </summary>
    [Column("link_status")]
    public ReportStatus LinkStatus { get; set; }

    /// <summary>
    /// get/set - The date and time the report was sent on (emailed out).
    /// </summary>
    [Column("link_sent_on")]
    public DateTime? LinkSentOn { get; set; }

    /// <summary>
    /// get/set - Response from CHES.
    /// </summary>
    [Column("link_response")]
    public JsonDocument LinkResponse { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The status of this report.
    /// </summary>
    [Column("text_status")]
    public ReportStatus TextStatus { get; set; }

    /// <summary>
    /// get/set - The date and time the report was sent on (emailed out).
    /// </summary>
    [Column("text_sent_on")]
    public DateTime? TextSentOn { get; set; }

    /// <summary>
    /// get/set - Response from CHES.
    /// </summary>
    [Column("text_response")]
    public JsonDocument TextResponse { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserReportInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="instance"></param>
    public UserReportInstance(User user, ReportInstance instance)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserReportInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="instanceId"></param>
    public UserReportInstance(int userId, long instanceId)
    {
        this.UserId = userId;
        this.InstanceId = instanceId;
    }
    #endregion

    #region Methods
    public bool Equals(UserReportInstance? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.InstanceId == other.InstanceId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserReport);
    public override int GetHashCode() => (this.UserId, this.InstanceId).GetHashCode();
    #endregion
}
