using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.DataSource;

/// <summary>
/// SourceMetricModel class, provides a model that represents an source metric.
/// </summary>
public class SourceMetricModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int DataSourceId { get; set; }

    /// <summary>
    /// get/set - The value of the reach.
    /// </summary>
    public float Reach { get; set; }

    /// <summary>
    /// get/set - The value of the earned.
    /// </summary>
    public float Earned { get; set; }

    /// <summary>
    /// get/set - The value of the impression.
    /// </summary>
    public float Impression { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceMetricModel.
    /// </summary>
    public SourceMetricModel() { }

    /// <summary>
    /// Creates a new instance of an SourceMetricModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceMetricModel(Entities.DataSourceMetric entity) : base(entity.SourceMetric)
    {
        this.Id = entity.SourceMetricId;
        this.DataSourceId = entity.DataSourceId;
        this.Reach = entity.Reach;
        this.Earned = entity.Earned;
        this.Impression = entity.Impression;
        this.CreatedBy = entity.CreatedBy;
        this.CreatedById = entity.CreatedById;
        this.CreatedOn = entity.CreatedOn;
        this.UpdatedBy = entity.UpdatedBy;
        this.UpdatedById = entity.UpdatedById;
        this.UpdatedOn = entity.UpdatedOn;
        this.Version = entity.Version;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a DataSourceMetric object.
    /// </summary>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public Entities.DataSourceMetric ToEntity(int dataSourceId)
    {
        var entity = (Entities.DataSourceMetric)this;
        entity.DataSourceId = dataSourceId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.DataSourceMetric(SourceMetricModel model)
    {
        return new Entities.DataSourceMetric(model.DataSourceId, model.Id, model.Reach, model.Earned, model.Impression)
        {
            Version = model.Version ?? 0
        };
    }
    #endregion
}
