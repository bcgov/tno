using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// SourceMetric class, provides an entity model that links (many-to-many) sources with metrics.
/// </summary>
[Table("source_metric")]
public class SourceMetric : AuditColumns, IEquatable<SourceMetric>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public virtual Source? Source { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the metric.
    /// </summary>
    [Column("metric_id")]
    public int MetricId { get; set; }

    /// <summary>
    /// get/set - The metric.
    /// </summary>
    public virtual Metric? Metric { get; set; }

    /// <summary>
    /// get/set - The metric reach.
    /// </summary>
    [Column("reach")]
    public float Reach { get; set; } = 0;

    /// <summary>
    /// get/set - The metric earned.
    /// </summary>
    [Column("earned")]
    public float Earned { get; set; } = 0;

    /// <summary>
    /// get/set - The metric impression.
    /// </summary>
    [Column("impression")]
    public float Impression { get; set; } = 0;
    #endregion

    #region Constructors
    protected SourceMetric() { }

    public SourceMetric(Source source, Metric metric, float reach, float earned, float impression)
    {
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
        this.MetricId = metric?.Id ?? throw new ArgumentNullException(nameof(metric));
        this.Metric = metric;
        this.Reach = reach;
        this.Earned = earned;
        this.Impression = impression;
    }

    public SourceMetric(int sourceId, int metricId, float reach, float earned, float impression)
    {
        this.SourceId = sourceId;
        this.MetricId = metricId;
        this.Reach = reach;
        this.Earned = earned;
        this.Impression = impression;
    }
    #endregion

    #region Methods
    public bool Equals(SourceMetric? other)
    {
        if (other == null) return false;
        return this.SourceId == other.SourceId && this.MetricId == other.MetricId;
    }

    public override bool Equals(object? obj) => Equals(obj as SourceMetric);
    public override int GetHashCode() => (this.SourceId, this.MetricId).GetHashCode();
    #endregion
}
