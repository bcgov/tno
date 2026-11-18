using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Setting class, provides a DB model to manage different types of settings.
/// </summary>
[Cache("setting")]
[Table("setting")]
public class Setting : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user who owns this setting.
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Setting object.
    /// </summary>
    protected Setting() : base() { }

    /// <summary>
    /// Creates a new instance of a Setting object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public Setting(string name, string value) : base(0, name)
    {
        this.Value = value;
    }

    /// <summary>
    /// Creates a new instance of a Setting object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public Setting(int id, string name, string value) : base(id, name)
    {
        this.Value = value;
    }
    #endregion
}
