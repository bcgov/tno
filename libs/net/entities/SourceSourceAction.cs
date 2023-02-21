using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// SourceSourceAction class, provides an entity model that links (many-to-many) sources with source actions.
/// </summary>
[Table("source_source_action")]
public class SourceSourceAction : AuditColumns, IEquatable<SourceSourceAction>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to source.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public virtual Source? Source { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the source action.
    /// </summary>
    [Column("source_action_id")]
    public int SourceActionId { get; set; }

    /// <summary>
    /// get/set - The source action.
    /// </summary>
    public virtual SourceAction? SourceAction { get; set; }

    /// <summary>
    /// get/set - The value of the source action.
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected SourceSourceAction() { }

    public SourceSourceAction(int dataSource, int actionId, string value)
    {
        this.SourceId = dataSource;
        this.SourceActionId = actionId;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public SourceSourceAction(Source dataSource, SourceAction action, string value)
    {
        this.SourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.Source = dataSource;
        this.SourceActionId = action?.Id ?? throw new ArgumentNullException(nameof(action));
        this.SourceAction = action;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion

    #region Methods
    public bool Equals(SourceSourceAction? other)
    {
        if (other == null) return false;
        return this.SourceId == other.SourceId && this.SourceActionId == other.SourceActionId;
    }

    public override bool Equals(object? obj) => Equals(obj as SourceSourceAction);
    public override int GetHashCode() => (this.SourceId, this.SourceActionId).GetHashCode();
    #endregion
}
