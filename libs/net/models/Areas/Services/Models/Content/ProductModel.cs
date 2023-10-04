using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ProductModel class, provides a model that represents an content type.
/// </summary>
public class ProductModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ProductModel.
    /// </summary>
    public ProductModel() { }

    /// <summary>
    /// Creates a new instance of an ProductModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ProductModel(Entities.Product entity) : base(entity)
    {
        this.AutoTranscribe = entity.AutoTranscribe;
    }
    #endregion
}
