using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.SourceMetric;

/// <summary>
/// SourceMetricModel class, provides a model that represents an source metric.
/// </summary>
public class SourceMetricModel : BaseTypeModel<int>
{
    #region Properties
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
    public SourceMetricModel(Entities.SourceMetric entity) : base(entity)
    {

    }
    #endregion
}
