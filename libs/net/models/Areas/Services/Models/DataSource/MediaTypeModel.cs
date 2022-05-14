using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// MediaTypeModel class, provides a model that represents an media type.
/// </summary>
public class MediaTypeModel : BaseTypeModel<int>
{
    #region Properties
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
    #endregion
}
