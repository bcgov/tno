using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Tag;

/// <summary>
/// TagModel class, provides a model that represents an tag.
/// </summary>
public class TagModel : BaseTypeModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    public string Code { get; set; } = "";

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
        this.Code = entity.Code;
    }
    #endregion
}
