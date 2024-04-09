using System.Text.Json;
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
    public string Key { get; set; } = "";

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
    /// get/set - The user preferences.
    /// </summary>
    public JsonDocument Preferences { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Number of allowed unique logins (0 means infinite).
    /// </summary>
    public int UniqueLogins { get; set; }

    /// <summary>
    /// get/set - An array of roles this user belongs to.
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - An array of organization this user belongs to.
    /// </summary>
    public IEnumerable<OrganizationModel> Organizations { get; set; } = Array.Empty<OrganizationModel>();

    /// <summary>
    /// get/set - An array of folders owned by this user.
    /// </summary>
    public IEnumerable<FolderModel> Folders { get; set; } = Array.Empty<FolderModel>();

    /// <summary>
    /// get/set - An array of filters owned by this user.
    /// </summary>
    public IEnumerable<FilterModel> Filters { get; set; } = Array.Empty<FilterModel>();

    /// <summary>
    /// get/set - An array of sources not accessible to this user.
    /// </summary>
    public IEnumerable<int> Sources { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of media types not accessible to this user.
    /// </summary>
    public IEnumerable<int> MediaTypes { get; set; } = Array.Empty<int>();
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
    /// <param name="serializerOptions"></param>
    public UserModel(Entities.User entity, JsonSerializerOptions? serializerOptions = null) : base(entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.PreferredEmail = entity.PreferredEmail;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.IsEnabled = entity.IsEnabled;
        this.Status = entity.Status;
        this.IsSystemAccount = entity.IsSystemAccount;
        this.EmailVerified = entity.EmailVerified;
        this.LastLoginOn = entity.LastLoginOn;
        this.Note = entity.Note;
        this.Preferences = entity.Preferences;
        this.UniqueLogins = entity.UniqueLogins;
        this.Roles = entity.Roles.Split(",").Where(s => !String.IsNullOrWhiteSpace(s)).Select(r => r[1..^1]);
        this.Organizations = entity.OrganizationsManyToMany.Where(o => o.Organization != null).Select(o => new OrganizationModel(o.Organization!));
        if (entity.Organizations.Any())
            this.Organizations = entity.Organizations.Select(o => new OrganizationModel(o));
        this.Folders = entity.Folders.Select(f => new FolderModel(f, serializerOptions ?? JsonSerializerOptions.Default));
        this.Filters = entity.Filters.Select(f => new FilterModel(f, serializerOptions ?? JsonSerializerOptions.Default));
        this.Sources = entity.SourcesManyToMany.Select(s => s.SourceId).ToArray();
        this.MediaTypes = entity.MediaTypesManyToMany.Select(s => s.MediaTypeId).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a User object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.User ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.User)this;
        entity.Folders.ForEach(s => s.Settings = JsonDocument.Parse(JsonSerializer.Serialize(s.Settings, options)));
        entity.Filters.ForEach(s =>
        {
            s.Query = JsonDocument.Parse(JsonSerializer.Serialize(s.Query, options));
            s.Settings = JsonDocument.Parse(JsonSerializer.Serialize(s.Settings, options));
        });
        return entity;
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
            PreferredEmail = model.PreferredEmail,
            DisplayName = model.DisplayName,
            IsEnabled = model.IsEnabled,
            Status = model.Status,
            IsSystemAccount = model.IsSystemAccount,
            EmailVerified = model.EmailVerified,
            LastLoginOn = model.LastLoginOn,
            Note = model.Note,
            Preferences = model.Preferences,
            UniqueLogins = model.UniqueLogins,
            Roles = String.Join(",", model.Roles.Select(r => $"[{r.ToLower()}]")),
            Version = model.Version ?? 0
        };

        entity.Folders.AddRange(model.Folders.Select(f => new Entities.Folder(f.Id, f.Name, f.OwnerId)
        {
            Description = f.Description,
            IsEnabled = f.IsEnabled,
            SortOrder = f.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(f.Settings)),
        }));

        entity.Filters.AddRange(model.Filters.Select(f => new Entities.Filter(f.Id, f.Name, f.OwnerId)
        {
            Description = f.Description,
            IsEnabled = f.IsEnabled,
            SortOrder = f.SortOrder,
            Query = JsonDocument.Parse(JsonSerializer.Serialize(f.Query)),
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(f.Settings)),
        }));

        entity.OrganizationsManyToMany.AddRange(model.Organizations.Select(o => new Entities.UserOrganization(entity.Id, o.Id)));
        entity.SourcesManyToMany.AddRange(model.Sources.Select(s => new Entities.UserSource(entity.Id, s)));
        entity.MediaTypesManyToMany.AddRange(model.MediaTypes.Select(s => new Entities.UserMediaType(entity.Id, s)));

        return entity;
    }
    #endregion
}
