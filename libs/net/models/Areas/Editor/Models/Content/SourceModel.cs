
using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Content;

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
    /// get/set -
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set -
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
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.SortOrder = entity.SortOrder;
        this.LicenseId = entity.LicenseId;
        this.OwnerId = entity.OwnerId;
        this.ProductId = entity.ProductId;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Source(SourceModel model)
    {
        return new Entities.Source(model.Name, model.Code, model.LicenseId)
        {
            Id = model.Id,
            ShortName = model.ShortName,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            OwnerId = model.OwnerId,
            ProductId = model.ProductId,
            AutoTranscribe = model.AutoTranscribe,
            DisableTranscribe = model.DisableTranscribe
        };
    }
    #endregion
}
