using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Action class, provides an entity for content actions.
/// An action is a way to identify content for processes, or filtering.
/// </summary>
[Cache("actions", "lookups")]
[Table("action")]
public class Action : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - A label to use when the value type is not boolean.
    /// </summary>
    [Column("value_label")]
    public string ValueLabel { get; set; } = "";

    /// <summary>
    /// get/set - The type of value this action will store.
    /// </summary>
    [Column("value_type")]
    public ValueType ValueType { get; set; }

    /// <summary>
    /// get/set - The default value to set for new content.
    /// </summary>
    [Column("default_value")]
    public string DefaultValue { get; set; } = "";

    /// <summary>
    /// get - Collection of contents associated to this action.
    /// Direction link to many-to-many.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - Collection of content actions associated to this action.
    /// </summary>
    public virtual List<ContentAction> ContentsManyToMany { get; } = new List<ContentAction>();

    /// <summary>
    /// get - Collection of content types that have this action.
    /// </summary>
    public virtual List<ContentType> ContentTypes { get; } = new List<ContentType>();

    /// <summary>
    /// get - Collection of content type actions.
    /// </summary>
    public virtual List<ContentTypeAction> ContentTypesManyToMany { get; } = new List<ContentTypeAction>();
    #endregion

    #region Constructors
    protected Action() { }

    public Action(string name, ValueType valueType, string valueLabel = "") : base(name)
    {
        this.ValueType = valueType;
        this.ValueLabel = valueLabel;
    }
    #endregion
}
