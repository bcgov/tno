using System.Text.Json;

namespace TNO.API.Areas.Editor.Models.DataSource;

/// <summary>
/// DataSourceModel class, provides a model that represents an category.
/// </summary>
public class DataSourceModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; } = default!;

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
    public int DataLocationId { get; set; }

    /// <summary>
    /// get/set - The name of the data location.
    /// </summary>
    public string DataLocation { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int? ContentTypeId { get; set; }

    /// <summary>
    /// get/set - The name of the content type.
    /// </summary>
    public string ContentType { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The name of the media type.
    /// </summary>
    public string MediaType { get; set; } = "";

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
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Connection { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceActionModel> Actions { get; } = Array.Empty<SourceActionModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an DataSourceModel.
    /// </summary>
    public DataSourceModel() { }

    /// <summary>
    /// Creates a new instance of an DataSourceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public DataSourceModel(Entities.DataSource entity, JsonSerializerOptions options)
    {
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.DataLocationId = entity.DataLocationId;
        this.DataLocation = entity.DataLocation?.Name ?? "";
        this.ContentTypeId = entity.ContentTypeId;
        this.ContentType = entity.ContentType?.Name ?? "";
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType?.Name ?? "";
        this.LicenseId = entity.LicenseId;
        this.License = entity.License?.Name ?? "";
        this.Topic = entity.Topic;
        this.ParentId = entity.ParentId;
        this.Connection = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Connection, options) ?? new Dictionary<string, object>();
        this.RetryLimit = entity.RetryLimit;
        this.LastRanOn = entity.DataService?.LastRanOn;
        this.FailedAttempts = entity.DataService?.FailedAttempts ?? 0;

        this.Actions = entity.ActionsManyToMany.Select(a => new SourceActionModel(a));
    }
    #endregion
}
