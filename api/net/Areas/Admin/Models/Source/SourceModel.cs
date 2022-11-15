using TNO.API.Areas.Admin.Models.Product;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Source;

/// <summary>
/// SourceModel class, provides a model that represents an source.
/// </summary>
public class SourceModel : AuditColumnsModel
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
    /// get/set - The license.
    /// </summary>
    public LicenseModel? License { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The default user who owns the content for this data source..
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// get/set - Whether content with this category should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether content with this category not be allowed to be requested for transcription.
    /// </summary>
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceSourceActionModel> Actions { get; set; } = Array.Empty<SourceSourceActionModel>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceMetricModel> Metrics { get; set; } = Array.Empty<SourceMetricModel>();
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
        this.LicenseId = entity.LicenseId;
        this.License = entity.License != null ? new LicenseModel(entity.License) : null;
        this.OwnerId = entity.OwnerId;
        this.ProductId = entity.ProductId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;

        this.Actions = entity.ActionsManyToMany.Select(a => new SourceSourceActionModel(a));
        this.Metrics = entity.MetricsManyToMany.Select(m => new SourceMetricModel(m));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Source object.
    /// </summary>
    /// <returns></returns>
    public Entities.Source ToEntity()
    {
        var entity = (Entities.Source)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Source(SourceModel model)
    {
        var entity = new Entities.Source(model.Name, model.Code, model.LicenseId)
        {
            Id = model.Id,
            ShortName = model.ShortName,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            ProductId = model.ProductId,
            AutoTranscribe = model.AutoTranscribe,
            DisableTranscribe = model.DisableTranscribe,
            Version = model.Version ?? 0
        };

        entity.ActionsManyToMany.AddRange(model.Actions.Select(a => a.ToEntity(entity.Id)));
        entity.MetricsManyToMany.AddRange(model.Metrics.Select(m => m.ToEntity(entity.Id)));

        return entity;
    }
    #endregion
}
