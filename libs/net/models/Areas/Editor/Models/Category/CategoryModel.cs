using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.Category;

/// <summary>
/// CategoryModel class, provides a model that represents an category.
/// </summary>
public class CategoryModel : BaseTypeModel<int>
{
    #region Properties
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
        this.CategoryType = entity.CategoryType;
        this.AutoTranscribe = entity.AutoTranscribe;
    }
    #endregion
}
