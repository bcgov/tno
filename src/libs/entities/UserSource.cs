using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserReport class, provides a model to link users with sources they do not have access to.
/// </summary>
[Table("user_source")]
public class UserSource : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the source.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source who is linked to the user.
    /// </summary>
    public Source? Source { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserSource object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="source"></param>
    public UserSource(User user, Source source)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserSource object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sourceId"></param>
    public UserSource(int userId, int sourceId)
    {
        this.UserId = userId;
        this.SourceId = sourceId;
    }
    #endregion

    #region Methods
    public bool Equals(UserSource? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.SourceId == other.SourceId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserSource);
    public override int GetHashCode() => (this.UserId, this.SourceId).GetHashCode();
    #endregion
}
