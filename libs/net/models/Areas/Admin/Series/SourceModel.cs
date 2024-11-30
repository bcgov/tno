namespace TNO.API.Areas.Admin.Models.Series;

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
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether content should not be allowed to be requested for transcription.
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
    public SourceModel(Entities.Source entity)
    {
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.LicenseId = entity.LicenseId;
        this.OwnerId = entity.OwnerId;
        this.MediaTypeId = entity.MediaTypeId;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
        this.UseInTopics = entity.UseInTopics;
        this.IsCBRASource = entity.IsCBRASource;
    }
    #endregion
}
