using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserDistribution class, provides a model to link users to users for distribution lists.
/// </summary>
[Table("user_distribution")]
public class UserDistribution : AuditColumns
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
    [Column("linked_user_id")]
    public int LinkedUserId { get; set; }

    /// <summary>
    /// get/set - the linked user.
    /// </summary>
    public User? LinkedUser { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserReport object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="linkedUser"></param>
    public UserDistribution(User user, User linkedUser)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.LinkedUser = linkedUser ?? throw new ArgumentNullException(nameof(linkedUser));
        this.LinkedUserId = linkedUser.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserReport object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="linkedUserId"></param>
    public UserDistribution(int userId, int linkedUserId)
    {
        this.UserId = userId;
        this.LinkedUserId = linkedUserId;
    }
    #endregion

    #region Methods
    public bool Equals(UserDistribution? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.LinkedUserId == other.LinkedUserId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserDistribution);
    public override int GetHashCode() => (this.UserId, this.LinkedUserId).GetHashCode();
    #endregion
}
