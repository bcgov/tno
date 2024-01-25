using TNO.API.Models;

namespace TNO.TemplateEngine.Models;

/// <summary>
/// MediaTypeModel class, provides a model that represents an content type.
/// </summary>
public class MediaTypeModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an MediaTypeModel.
    /// </summary>
    public MediaTypeModel() { }

    /// <summary>
    /// Creates a new instance of an MediaTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public MediaTypeModel(Entities.MediaType entity) : base(entity)
    {
    }

    /// <summary>
    /// Creates a new instance of an MediaTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public MediaTypeModel(TNO.API.Areas.Editor.Models.Content.MediaTypeModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.AutoTranscribe = model.AutoTranscribe;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }

    /// <summary>
    /// Creates a new instance of an MediaTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public MediaTypeModel(TNO.API.Areas.Services.Models.Content.MediaTypeModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.AutoTranscribe = model.AutoTranscribe;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }

    /// <summary>
    /// Creates a new instance of an MediaTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public MediaTypeModel(TNO.API.Areas.Services.Models.Report.MediaTypeModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.AutoTranscribe = model.AutoTranscribe;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }

    /// <summary>
    /// Creates a new instance of an MediaTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public MediaTypeModel(TNO.API.Areas.Services.Models.ReportInstance.MediaTypeModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.AutoTranscribe = model.AutoTranscribe;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }
    #endregion
}
