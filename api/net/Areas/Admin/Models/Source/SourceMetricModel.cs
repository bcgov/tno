using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Source;

/// <summary>
/// SourceMetricModel class, provides a model that represents an source metric.
/// </summary>
public class SourceMetricModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int SourceId { get; set; }

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
    public SourceMetricModel(Entities.SourceMetric entity) : base(entity.Metric)
    {
        this.Id = entity.MetricId;
        this.SourceId = entity.SourceId;
        this.Reach = entity.Reach;
        this.Earned = entity.Earned;
        this.Impression = entity.Impression;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a SourceMetric object.
    /// </summary>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public Entities.SourceMetric ToEntity(int dataSourceId)
    {
        var entity = (Entities.SourceMetric)this;
        entity.SourceId = dataSourceId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.SourceMetric(SourceMetricModel model)
    {
        return new Entities.SourceMetric(model.SourceId, model.Id, model.Reach, model.Earned, model.Impression)
        {
            Version = model.Version ?? 0
        };
    }
    #endregion
}
