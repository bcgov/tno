using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Report;

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
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");
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
    public FolderModel(Entities.Folder entity) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Settings = entity.Settings;
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

        return entity;
    }
    #endregion
}
