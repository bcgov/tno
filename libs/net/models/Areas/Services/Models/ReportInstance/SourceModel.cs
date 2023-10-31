
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// SourceModel class, provides a model that represents an source.
/// </summary>
public class SourceModel : BaseTypeModel<int>
{
    #region Properties

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
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// get/set - Whether this source auto transcribes A/V files.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether this source disabled transcription requests.
    /// </summary>
    public bool DisableTranscribe { get; set; }
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
    public SourceModel(Entities.Source entity) : base(entity)
    {
        this.Code = entity.Code;
        this.ShortName = entity.ShortName;
        this.LicenseId = entity.LicenseId;
        this.OwnerId = entity.OwnerId;
        this.ProductId = entity.ProductId;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
    }

    /// <summary>
    /// Creates a new instance of an SourceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public SourceModel(TNO.API.Areas.Services.Models.Content.SourceModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Code = model.Code;
        this.ShortName = model.ShortName;
        this.LicenseId = model.LicenseId;
        this.OwnerId = model.OwnerId;
        this.ProductId = model.ProductId;
        this.AutoTranscribe = model.AutoTranscribe;
        this.DisableTranscribe = model.DisableTranscribe;
    }
    #endregion
}
