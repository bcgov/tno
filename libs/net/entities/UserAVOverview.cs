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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserAVOverview object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="template"></param>
    public UserAVOverview(User user, AVOverviewTemplate template)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
        this.TemplateType = template.TemplateType;
        this.IsSubscribed = true;
    }

    /// <summary>
    /// Creates a new instance of a UserAVOverview object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="templateType"></param>
    public UserAVOverview(int userId, AVOverviewTemplateType templateType)
    {
        this.UserId = userId;
        this.TemplateType = templateType;
        this.IsSubscribed = true;
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
