using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Metric;

/// <summary>
/// MetricModel class, provides a model that represents an metric.
/// </summary>
public class MetricModel : BaseTypeModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an MetricModel.
    /// </summary>
    public MetricModel() { }

    /// <summary>
    /// Creates a new instance of an MetricModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public MetricModel(Entities.Metric entity) : base(entity)
    {

    }
    #endregion
}
