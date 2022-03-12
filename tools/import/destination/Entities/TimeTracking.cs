using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("time_tracking")]
public class TimeTracking : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Column("effort")]
    public float Effort { get; set; }

    [Column("activity")]
    public string Activity { get; set; } = "";
    #endregion

    #region Constructors
    protected TimeTracking() { }

    public TimeTracking(Content content, User user)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.UserId = user?.Id ?? throw new ArgumentNullException(nameof(user));
        this.User = user;
    }

    public TimeTracking(int contentId, int userId)
    {
        this.ContentId = contentId;
        this.UserId = userId;
    }
    #endregion
}