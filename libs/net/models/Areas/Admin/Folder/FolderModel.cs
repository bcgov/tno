using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Folder;

/// <summary>
/// FolderModel class, provides a model that represents an folder.
/// </summary>
public class FolderModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of this report.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - The folder settings.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set - An array of content in this folder.
    /// </summary>
    public IEnumerable<FolderContentModel> Content { get; set; } = Array.Empty<FolderContentModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FolderModel.
    /// </summary>
    public FolderModel() { }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public FolderModel(Entities.Folder entity, JsonSerializerOptions options) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();
        this.Content = entity.ContentManyToMany.Select(c => new FolderContentModel(c));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Folder object.
    /// </summary>
    /// <returns></returns>
    public Entities.Folder ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Folder)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Folder(FolderModel model)
    {
        var entity = new Entities.Folder(model.Id, model.Name, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        entity.ContentManyToMany.AddRange(model.Content.Select(c => new Entities.FolderContent(model.Id, c.ContentId, c.SortOrder)));

        return entity;
    }
    #endregion
}
