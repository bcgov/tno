using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Services.Models.Content;

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
    /// get/set - Media Type settings.
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
}
