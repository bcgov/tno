using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserMediaType class, provides a model to link users with media types they do not have access to.
/// </summary>
[Table("user_media_type")]
public class UserMediaType : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the media_type.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the media_type.
    /// </summary>
    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The media_type who is linked to the user.
    /// </summary>
    public MediaType? MediaType { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserMediaType object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="mediaType"></param>
    public UserMediaType(User user, MediaType mediaType)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaTypeId = mediaType.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserMediaType object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="mediaTypeId"></param>
    public UserMediaType(int userId, int mediaTypeId)
    {
        this.UserId = userId;
        this.MediaTypeId = mediaTypeId;
    }
    #endregion

    #region Methods
    public bool Equals(UserMediaType? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.MediaTypeId == other.MediaTypeId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserMediaType);
    public override int GetHashCode() => (this.UserId, this.MediaTypeId).GetHashCode();
    #endregion
}
