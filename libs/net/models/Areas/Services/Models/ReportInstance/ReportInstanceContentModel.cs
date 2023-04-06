using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// ReportInstanceContentModel class, provides a model that represents an report instance content relationship.
/// </summary>
public class ReportInstanceContentModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report definition.
    /// </summary>
    public long ContentId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportInstanceContentModel.
    /// </summary>
    public ReportInstanceContentModel() { }

    /// <summary>
    /// Creates a new instance of an ReportInstanceContentModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportInstanceContentModel(Entities.ReportInstanceContent entity) : base(entity)
    {
        this.InstanceId = entity.InstanceId;
        this.ContentId = entity.ContentId;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ReportInstanceContent(ReportInstanceContentModel model)
    {
        return new Entities.ReportInstanceContent(model.InstanceId, model.ContentId)
        {
            Version = model.Version ?? 0
        };
    }
    #endregion
}
