using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.MediaType;

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

    /// <summary>
    /// get/set - The type of entity that should be listed with this media type
    /// </summary>
    public Entities.ListOption ListOption { get; set; }
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
        this.ListOption = entity.ListOption;
    }
    #endregion
}
