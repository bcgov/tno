using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Category;

/// <summary>
/// CategoryModel class, provides a model that represents an category.
/// </summary>
public class CategoryModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The type of category (issue, proactive).
    /// </summary>
    public CategoryType CategoryType { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an CategoryModel.
    /// </summary>
    public CategoryModel() { }

    /// <summary>
    /// Creates a new instance of an CategoryModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public CategoryModel(Entities.Category entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
        this.CategoryType = entity.CategoryType;
        this.AutoTranscribe = entity.AutoTranscribe;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Category(CategoryModel model)
    {
        var entity = new Entities.Category(model.Name, model.CategoryType)
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            AutoTranscribe = model.AutoTranscribe,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
