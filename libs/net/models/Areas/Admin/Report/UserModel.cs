using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// UserModel class, provides a model that represents an user.
/// </summary>
public class UserModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique username to identify user.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// get/set - The user's preferred email address.
    /// </summary>
    public string PreferredEmail { get; set; } = "";

    /// <summary>
    /// get/set - Display name of user.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// get/set - First name of user.
    /// </summary>
    public string FirstName { get; set; } = "";

    /// <summary>
    /// get/set - Last name of user.
    /// </summary>
    public string LastName { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserModel.
    /// </summary>
    public UserModel() { }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserModel(Entities.User? entity) : base(entity)
    {
        if (entity != null)
        {
            this.Id = entity.Id;
            this.Username = entity.Username;
            this.Email = entity.Email;
            this.PreferredEmail = entity.PreferredEmail;
            this.DisplayName = entity.DisplayName;
            this.FirstName = entity.FirstName;
            this.LastName = entity.LastName;
        }
    }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserModel(Entities.UserReport entity) : base(entity)
    {
        this.Id = entity.UserId;
        if (entity.User != null)
        {
            this.Username = entity.User.Username;
            this.Email = entity.User.Email;
            this.PreferredEmail = entity.User.PreferredEmail;
            this.DisplayName = entity.User.DisplayName;
            this.FirstName = entity.User.FirstName;
            this.LastName = entity.User.LastName;
        }
    }
    #endregion
}
