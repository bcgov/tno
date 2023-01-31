using System.Text.Json;

namespace TNO.API.Areas.Editor.Models.Source;

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
    public int? ProductId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceSourceActionModel> Actions { get; } = Array.Empty<SourceSourceActionModel>();
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
        this.ProductId = entity.ProductId;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();

        this.Actions = entity.ActionsManyToMany.Select(a => new SourceSourceActionModel(a));
    }
    #endregion
}
