using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Category;

/// <summary>
/// CategoryModel class, provides a model that represents an category.
/// </summary>
public class CategoryModel : BaseTypeModel<int>
{
    #region Properties
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

    }
    #endregion
}
