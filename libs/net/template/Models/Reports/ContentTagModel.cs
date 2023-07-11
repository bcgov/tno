namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ContentTagModel class, provides a model that represents an tag.
/// </summary>
public class ContentTagModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTagModel.
    /// </summary>
    public ContentTagModel()
    {
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="code"></param>
    public ContentTagModel(string code)
    {
        this.Code = code;
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTagModel(Entities.ContentTag entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TagId;
        this.Code = entity.Tag?.Code ?? "";
        this.Name = entity.Tag?.Name ?? "";
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTagModel(TNO.API.Areas.Editor.Models.Content.ContentTagModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Code = model.Code;
        this.Name = model.Name;
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTagModel(TNO.API.Areas.Services.Models.Content.ContentTagModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Code = model.Code;
        this.Name = model.Name;
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTagModel(TNO.API.Areas.Services.Models.ReportInstance.ContentTagModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Code = model.Code;
        this.Name = model.Name;
    }
    #endregion
}
