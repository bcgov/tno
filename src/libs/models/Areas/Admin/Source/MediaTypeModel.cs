using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Source;

/// <summary>
/// MediaTypeModel class, provides a model that represents an media type.
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
        this.AutoTranscribe = entity.AutoTranscribe;
    }

    /// <summary>
    /// Creates a new instance of a MediaType object.
    /// </summary>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public Entities.MediaType ToEntity()
    {
        var entity = (Entities.MediaType)this;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.MediaType(MediaTypeModel model)
    {
        return new Entities.MediaType(model.Name);
    }

    #endregion
}
