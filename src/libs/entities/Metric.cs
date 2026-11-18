using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Metric class, provides an entity model that represents the many-to-many between source and metric.
/// </summary>
[Cache("metrics", "lookups")]
[Table("metric")]
public class Metric : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get - List of linked sources.
    /// </summary>
    public virtual List<Source> Sources { get; } = new List<Source>();

    /// <summary>
    /// get - List of linked sources (many-to-many).
    /// </summary>
    public virtual List<SourceMetric> SourcesManyToMany { get; } = new List<SourceMetric>();
    #endregion

    #region Constructors
    protected Metric() { }

    public Metric(string name) : base(name)
    {
    }
    #endregion
}
