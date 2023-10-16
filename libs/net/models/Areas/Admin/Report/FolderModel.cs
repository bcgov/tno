using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Report;

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
    /// get/set - An array of content in this folder.
    /// </summary>
    public IEnumerable<FolderContentModel> Content { get; set; } = Array.Empty<FolderContentModel>();

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
    /// <param name="options"></param>
    public FolderModel(Entities.Folder entity, JsonSerializerOptions options) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Content = entity.ContentManyToMany.Select(c => new FolderContentModel(c, options));
        this.Settings = entity.Settings;
    }
    #endregion

    #region Methods
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
            Settings = model.Settings,
            Version = model.Version ?? 0
        };

        entity.ContentManyToMany.AddRange(model.Content.Select(c => (Entities.FolderContent)c));

        return entity;
    }
    #endregion
}
