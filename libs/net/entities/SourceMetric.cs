using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("source_metrics", "lookups")]
[Table("source_metric")]
public class SourceMetric : BaseType<int>
{
    #region Properties
    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();
    public virtual List<DataSourceMetric> DataSourcesManyToMany { get; set; } = new List<DataSourceMetric>();
    #endregion

    #region Constructors
    protected SourceMetric() { }

    public SourceMetric(string name) : base(name)
    {
    }
    #endregion
}
