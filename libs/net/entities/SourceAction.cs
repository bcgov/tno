using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("source_actions", "lookups")]
[Table("source_action")]
public class SourceAction : BaseType<int>
{
    #region Properties
    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();
    public virtual List<DataSourceAction> DataSourcesManyToMany { get; set; } = new List<DataSourceAction>();
    #endregion

    #region Constructors
    protected SourceAction() { }

    public SourceAction(string name) : base(name)
    {
    }
    #endregion
}
