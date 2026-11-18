using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserReport class, provides a model to link users with their colleagues.
/// </summary>
[Table("user_colleague")]
public class UserColleague : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the colleague.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the colleague.
    /// </summary>
    [Column("colleague_id")]
    public int ColleagueId { get; set; }

    /// <summary>
    /// get/set - The colleague who is linked to the user.
    /// </summary>
    public User? Colleague { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserColleague object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="colleague"></param>
    public UserColleague(User user, User colleague)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Colleague = colleague ?? throw new ArgumentNullException(nameof(colleague));
        this.ColleagueId = colleague.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserColleague object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="colleagueId"></param>
    public UserColleague(int userId, int colleagueId)
    {
        this.UserId = userId;
        this.ColleagueId = colleagueId;
    }
    #endregion

    #region Methods
    public bool Equals(UserColleague? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.ColleagueId == other.ColleagueId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserColleague);
    public override int GetHashCode() => (this.UserId, this.ColleagueId).GetHashCode();
    #endregion
}
