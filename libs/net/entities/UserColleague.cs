using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserColleague class, provides a model to link users with their colleagues.
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
    /// get/set - the colleague linked to the user.
    /// </summary>
    public User? ColleagueUser { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserColleague object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="organization"></param>
    public UserColleague(User user, User colleagueUser)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.ColleagueUser = colleagueUser ?? throw new ArgumentNullException(nameof(colleagueUser));
        this.ColleagueId = colleagueUser.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserColleague object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="organizationId"></param>
    public UserColleague(int userId, int colleagueUserId)
    {
        this.UserId = userId;
        this.ColleagueId = colleagueUserId;
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
