using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// SourceAction class, provides an entity model that represents the possible actions a source can have.
/// </summary>
[Cache("source_actions", "lookups")]
[Table("source_action")]
public class SourceAction : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get - List of linked sources.
    /// </summary>
    public virtual List<Source> Sources { get; } = new List<Source>();

    /// <summary>
    /// get - List of linked sources (many-to-many).
    /// </summary>
    public virtual List<SourceSourceAction> SourcesManyToMany { get; } = new List<SourceSourceAction>();
    #endregion

    #region Constructors
    protected SourceAction() { }

    public SourceAction(string name) : base(name)
    {
    }
    #endregion
}
