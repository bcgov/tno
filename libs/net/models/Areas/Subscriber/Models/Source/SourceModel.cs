using System.Text.Json;
using TNO.API.Areas.Subscriber.Models.MediaType;

namespace TNO.API.Areas.Subscriber.Models.Source;

/// <summary>
/// SourceModel class, provides a model that represents an source.
/// </summary>
public class SourceModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string ShortName { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - The name of the license.
    /// </summary>
    public string License { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Collection of media types used in search mapping, the many-to-many relationship.
    /// </summary>
    public IEnumerable<MediaTypeModel> MediaTypeSearchMappings { get; set; } = Array.Empty<MediaTypeModel>();

    /// <summary>
    /// get/set -
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether to show the topics on the content form.
    /// </summary>
    public bool UseInTopics { get; set; }

    /// <summary>
    /// get/set - is CBRA source or not.
    /// </summary>
    public bool IsCBRASource { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceModel.
    /// </summary>
    public SourceModel() { }

    /// <summary>
    /// Creates a new instance of an SourceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public SourceModel(Entities.Source entity, JsonSerializerOptions options)
    {
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.SortOrder = entity.SortOrder;
        this.LicenseId = entity.LicenseId;
        this.License = entity.License?.Name ?? "";
        this.OwnerId = entity.OwnerId;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaTypeSearchMappings = entity.MediaTypeSearchMappingsManyToMany.Select(m => new MediaTypeModel(m.MediaType!));
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
        this.UseInTopics = entity.UseInTopics;
        this.IsCBRASource = entity.IsCBRASource;
    }
    #endregion
}
