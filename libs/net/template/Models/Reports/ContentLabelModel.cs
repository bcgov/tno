namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ContentLabelModel class, provides a model that represents a label to identify content information.
/// </summary>
public class ContentLabelModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to label.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The key to group related labels.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The value of the label.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentLabelModel.
    /// </summary>
    public ContentLabelModel() { }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel.
    /// </summary>
    public ContentLabelModel(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentLabelModel(Entities.ContentLabel entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.Key = entity.Key;
        this.Value = entity.Value;
    }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentLabelModel(TNO.API.Areas.Editor.Models.Content.ContentLabelModel model)
    {
        this.Id = model.Id;
        this.ContentId = model.ContentId;
        this.Key = model.Key;
        this.Value = model.Value;
    }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentLabelModel(TNO.API.Areas.Services.Models.Content.ContentLabelModel model)
    {
        this.Id = model.Id;
        this.ContentId = model.ContentId;
        this.Key = model.Key;
        this.Value = model.Value;
    }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentLabelModel(TNO.API.Areas.Services.Models.ReportInstance.ContentLabelModel model)
    {
        this.Id = model.Id;
        this.ContentId = model.ContentId;
        this.Key = model.Key;
        this.Value = model.Value;
    }
    #endregion
}
