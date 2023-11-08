using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Admin.Models.MediaType;

/// <summary>
/// MediaTypeModel class, provides a model that represents an media type.
/// </summary>
public class MediaTypeModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - MediaType settings.
    /// </summary>
    public MediaTypeSettingsModel Settings { get; set; } = new MediaTypeSettingsModel();
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
    /// <param name="options"></param>
    public MediaTypeModel(Entities.MediaType entity, JsonSerializerOptions options) : base(entity)
    {
        this.AutoTranscribe = entity.AutoTranscribe;
        this.Settings = JsonSerializer.Deserialize<MediaTypeSettingsModel>(entity.Settings, options) ?? new();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a MediaType object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.MediaType ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.MediaType)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.MediaType(MediaTypeModel model)
    {
        var entity = new Entities.MediaType(model.Name)
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
