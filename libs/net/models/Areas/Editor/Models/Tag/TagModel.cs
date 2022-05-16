using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Tag;

/// <summary>
/// TagModel class, provides a model that represents an tag.
/// </summary>
public class TagModel : BaseTypeModel<string>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TagModel.
    /// </summary>
    public TagModel() { }

    /// <summary>
    /// Creates a new instance of an TagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TagModel(Entities.Tag entity) : base(entity)
    {

    }
    #endregion
}
