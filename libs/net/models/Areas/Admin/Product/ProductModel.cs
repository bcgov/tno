using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Admin.Models.Product;

/// <summary>
/// ProductModel class, provides a model that represents an product.
/// </summary>
public class ProductModel : BaseTypeWithAuditColumnsModel<int>
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

    #region Methods
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.Product ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Product)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Product(ProductModel model)
    {
        var entity = new Entities.Product(model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            AutoTranscribe = model.AutoTranscribe,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
