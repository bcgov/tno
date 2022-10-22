using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserModel class, provides a model that represents an user.
/// </summary>
public class UserModel : AuditColumnsModel
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
    /// get/set - The user status.
    /// </summary>
    public Entities.UserStatus Status { get; set; }

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

    /// <summary>
    /// get/set - A user note.
    /// </summary>
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - An array of roles this user belongs to.
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
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
    public UserModel(Entities.User entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.IsEnabled = entity.IsEnabled;
        this.Status = entity.Status;
        this.IsSystemAccount = entity.IsSystemAccount;
        this.EmailVerified = entity.EmailVerified;
        this.LastLoginOn = entity.LastLoginOn;
        this.Note = entity.Note;
        this.Roles = entity.Roles.Split(",").Where(s => !String.IsNullOrWhiteSpace(s)).Select(r => r[1..^1]);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a User object.
    /// </summary>
    /// <returns></returns>
    public Entities.User ToEntity()
    {
        return (Entities.User)this;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.User(UserModel model)
    {
        var entity = new Entities.User(model.Username, model.Email, model.Key)
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            DisplayName = model.DisplayName,
            IsEnabled = model.IsEnabled,
            Status = model.Status,
            IsSystemAccount = model.IsSystemAccount,
            EmailVerified = model.EmailVerified,
            LastLoginOn = model.LastLoginOn,
            Note = model.Note,
            Roles = String.Join(",", model.Roles.Select(r => $"[{r.ToLower()}]")),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
