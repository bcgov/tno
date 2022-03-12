using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("action")]
public class Action : BaseType<int>
{
    #region Properties
    /// <summary>
    /// A label to use when the value type is not boolean.
    /// </summary>
    [Column("value_label")]
    public string ValueLabel { get; set; } = "";

    /// <summary>
    /// The type of value this action will store.
    /// </summary>
    [Column("value_type")]
    public ValueType ValueType { get; set; }

    public List<ContentAction> ContentActions { get; } = new List<ContentAction>();

    public List<ContentType> ContentTypes { get; } = new List<ContentType>();
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