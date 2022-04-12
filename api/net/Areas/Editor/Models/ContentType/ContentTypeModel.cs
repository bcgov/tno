using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.ContentType;

/// <summary>
/// ContentTypeModel class, provides a model that represents an content type.
/// </summary>
public class ContentTypeModel : BaseTypeModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTypeModel.
    /// </summary>
    public ContentTypeModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTypeModel(Entities.ContentType entity) : base(entity)
    {

    }
    #endregion
}
