using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models;

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

    /// <summary>
    /// get/set - The preferences for this user.
    /// </summary>
    public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();
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
    /// <param name="options"></param>
    public UserModel(Entities.User entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.PreferredEmail = entity.PreferredEmail;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.Preferences = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Preferences, options) ?? new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a User object.
    /// </summary>
    /// <returns></returns>
    public Entities.User ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.User)this;
        entity.Preferences = JsonDocument.Parse(JsonSerializer.Serialize(this.Preferences, options));
        return entity;

    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.User(UserModel model)
    {
        var entity = new Entities.User(model.Username, model.Email)
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PreferredEmail = model.PreferredEmail,
            DisplayName = model.DisplayName,
            Preferences = JsonDocument.Parse(JsonSerializer.Serialize(model.Preferences)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
