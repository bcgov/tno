using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.User;

/// <summary>
/// UserModel class, provides a model that represents an user.
/// </summary>
public class UserModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public int Id { get; set; } = default!;

    /// <summary>
    /// get/set - Unique key to identify the user.
    /// </summary>
    public Guid Key { get; set; } = Guid.Empty;

    /// <summary>
    /// get/set - Unique username to identify user.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string Email { get; set; } = "";

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

    /// <summary>
    /// get/set - Whether the user is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - Whether the user email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// get/set - Whether the user is a system account.
    /// </summary>
    public bool IsSystemAccount { get; set; }

    /// <summary>
    /// get/set - The last date and time when user logged in.
    /// </summary>
    public DateTime? LastLoginOn { get; set; }
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
    public UserModel(Entities.User entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.IsEnabled = entity.IsEnabled;
        this.IsSystemAccount = entity.IsSystemAccount;
        this.EmailVerified = entity.EmailVerified;
        this.LastLoginOn = entity.LastLoginOn;
    }
    #endregion
}
