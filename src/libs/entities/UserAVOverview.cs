using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserAVOverview class, provides a model to link users with their reports.
/// </summary>
[Table("user_av_overview")]
public class UserAVOverview : AuditColumns
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
    [Column("av_overview_template_type")]
    public AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - the report linked to the user.
    /// </summary>
    public AVOverviewTemplate? Template { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to this report.
    /// </summary>
    [Column("is_subscribed")]
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// get/set - How the email will be sent to the subscriber.
    /// </summary>
    [Column("send_to")]
    public EmailSentTo SendTo { get; set; } = EmailSentTo.To;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserAVOverview object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="template"></param>
    /// <param name="isSubscribed"></param>
    /// <param name="sendTo"></param>
    public UserAVOverview(User user, AVOverviewTemplate template, bool isSubscribed = true, EmailSentTo sendTo = EmailSentTo.To)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
        this.TemplateType = template.TemplateType;
        this.IsSubscribed = isSubscribed;
        this.SendTo = sendTo;
    }

    /// <summary>
    /// Creates a new instance of a UserAVOverview object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="templateType"></param>
    /// <param name="isSubscribed"></param>
    /// <param name="sendTo"></param>
    public UserAVOverview(int userId, AVOverviewTemplateType templateType, bool isSubscribed = true, EmailSentTo sendTo = EmailSentTo.To)
    {
        this.UserId = userId;
        this.TemplateType = templateType;
        this.IsSubscribed = isSubscribed;
        this.SendTo = sendTo;
    }
    #endregion

    #region Methods
    public bool Equals(UserAVOverview? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.TemplateType == other.TemplateType;
    }

    public override bool Equals(object? obj) => Equals(obj as UserAVOverview);
    public override int GetHashCode() => (this.UserId, this.TemplateType).GetHashCode();
    #endregion
}
