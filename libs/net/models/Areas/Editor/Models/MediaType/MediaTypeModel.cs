using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.MediaType;

/// <summary>
/// MediaTypeModel class, provides a model that represents an media type.
/// </summary>
public class MediaTypeModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The content type of this media and the form to use.
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Snippet;
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
        this.ContentType = entity.ContentType;
    }
    #endregion
}
