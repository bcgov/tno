using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

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

    /// <summary>
    /// get/set - Product settings.
    /// </summary>
    public ProductSettingsModel Settings { get; set; } = new ProductSettingsModel();
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
    /// <param name="options"></param>
    public ProductModel(Entities.Product entity, JsonSerializerOptions options) : base(entity)
    {
        this.AutoTranscribe = entity.AutoTranscribe;
        this.Settings = JsonSerializer.Deserialize<ProductSettingsModel>(entity.Settings, options) ?? new();
    }
    #endregion
}
