using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("data_source_action")]
public class DataSourceAction : AuditColumns, IEquatable<DataSourceAction>
{
    #region Properties
    [Column("data_source_id")]
    public int DataSourceId { get; set; }

    public virtual DataSource? DataSource { get; set; }

    [Column("source_action_id")]
    public int SourceActionId { get; set; }

    public virtual SourceAction? SourceAction { get; set; }

    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected DataSourceAction() { }

    public DataSourceAction(DataSource dataSource, SourceAction action, string value)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
        this.SourceActionId = action?.Id ?? throw new ArgumentNullException(nameof(action));
        this.SourceAction = action;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public DataSourceAction(int dataSource, int actionId, string value)
    {
        this.DataSourceId = dataSource;
        this.SourceActionId = actionId;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion

    #region Methods
    public bool Equals(DataSourceAction? other)
    {
        if (other == null) return false;
        return this.DataSourceId == other.DataSourceId && this.SourceActionId == other.SourceActionId;
    }

    public override bool Equals(object? obj) => Equals(obj as DataSourceAction);
    public override int GetHashCode() => (this.DataSourceId, this.SourceActionId).GetHashCode();
    #endregion
}
