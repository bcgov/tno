using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("data_source_metric")]
public class DataSourceMetric : AuditColumns, IEquatable<DataSourceMetric>
{
    #region Properties
    [Column("data_source_id")]
    public int DataSourceId { get; set; }

    public virtual DataSource? DataSource { get; set; }

    [Column("source_metric_id")]
    public int SourceMetricId { get; set; }

    public virtual SourceMetric? SourceMetric { get; set; }

    [Column("reach")]
    public float Reach { get; set; } = 0;

    [Column("earned")]
    public float Earned { get; set; } = 0;

    [Column("impression")]
    public float Impression { get; set; } = 0;
    #endregion

    #region Constructors
    protected DataSourceMetric() { }

    public DataSourceMetric(DataSource dataSource, SourceMetric metric, float reach, float earned, float impression)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
        this.SourceMetricId = metric?.Id ?? throw new ArgumentNullException(nameof(metric));
        this.SourceMetric = metric;
        this.Reach = reach;
        this.Earned = earned;
        this.Impression = impression;
    }

    public DataSourceMetric(int dataSourceId, int metricId, float reach, float earned, float impression)
    {
        this.DataSourceId = dataSourceId;
        this.SourceMetricId = metricId;
        this.Reach = reach;
        this.Earned = earned;
        this.Impression = impression;
    }
    #endregion

    #region Methods
    public bool Equals(DataSourceMetric? other)
    {
        if (other == null) return false;
        return this.DataSourceId == other.DataSourceId && this.SourceMetricId == other.SourceMetricId;
    }

    public override bool Equals(object? obj) => Equals(obj as DataSourceMetric);
    public override int GetHashCode() => (this.DataSourceId, this.SourceMetricId).GetHashCode();
    #endregion
}
