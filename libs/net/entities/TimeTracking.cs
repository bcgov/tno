using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("time_tracking")]
public class TimeTracking : AuditColumns
{
    #region Properties
    [Column("id")]
    public long Id { get; set; }

    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    public virtual User? User { get; set; }

    [Column("effort")]
    public float Effort { get; set; }

    [Column("activity")]
    public string Activity { get; set; } = "";
    #endregion

    #region Constructors
    protected TimeTracking() { }

    public TimeTracking(Content content, User user, float effort, string? activity = null)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.UserId = user?.Id ?? throw new ArgumentNullException(nameof(user));
        this.User = user;
        this.Effort = effort;
        this.Activity = activity ?? "";
    }

    public TimeTracking(long contentId, int userId, float effort, string? activity = null)
    {
        this.ContentId = contentId;
        this.UserId = userId;
        this.Effort = effort;
        this.Activity = activity ?? "";
    }
    #endregion
}
