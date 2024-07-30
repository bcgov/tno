using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// UserAVOverviewInstance class, provides a model to link users with their report instances to track email status.
/// </summary>
[Table("user_av_overview_instance")]
public class UserAVOverviewInstance : AuditColumns
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
    public AVOverviewInstance? Instance { get; set; }

    /// <summary>
    /// get/set - The date and time the report was sent on (emailed out).
    /// </summary>
    [Column("sent_on")]
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The status of this report.
    /// </summary>
    [Column("status")]
    public ReportStatus Status { get; set; }

    /// <summary>
    /// get/set - Response from CHES.
    /// </summary>
    [Column("response")]
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserAVOverviewInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="instance"></param>
    /// <param name="sentOn"></param>
    /// <param name="status"></param>
    /// <param name="response"></param>
    public UserAVOverviewInstance(User user, AVOverviewInstance instance, DateTime? sentOn, ReportStatus status, JsonDocument response)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.SentOn = sentOn;
        this.Status = status;
        this.Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Creates a new instance of a UserAVOverviewInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="instanceId"></param>
    /// <param name="sentOn"></param>
    /// <param name="status"></param>
    /// <param name="response"></param>
    public UserAVOverviewInstance(int userId, long instanceId, DateTime? sentOn, ReportStatus status, JsonDocument response)
    {
        this.UserId = userId;
        this.InstanceId = instanceId;
        this.SentOn = sentOn;
        this.Status = status;
        this.Response = response ?? throw new ArgumentNullException(nameof(response));
    }
    #endregion

    #region Methods
    public bool Equals(UserAVOverviewInstance? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.InstanceId == other.InstanceId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserAVOverviewInstance);
    public override int GetHashCode() => (this.UserId, this.InstanceId).GetHashCode();
    #endregion
}
