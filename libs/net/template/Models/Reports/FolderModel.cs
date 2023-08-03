using System.Text.Json;
using TNO.API.Models;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// FolderModel class, provides a model that represents an folder.
/// </summary>
public class FolderModel : BaseTypeModel<int>
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

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FolderModel(TNO.API.Areas.Admin.Models.Report.FolderModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FolderModel(TNO.API.Areas.Editor.Models.Report.FolderModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FolderModel(TNO.API.Areas.Services.Models.Report.FolderModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }
    #endregion
}
