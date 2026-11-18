using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// License class, provides an entity model to manage different licenses.
/// </summary>
[Cache("licenses", "lookups")]
[Table("license")]
public class License : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Time to live.
    /// </summary>
    [Column("ttl")]
    public int TTL { get; set; }

    /// <summary>
    /// get - List of source linked to this license.
    /// </summary>
    public virtual List<Source> Sources { get; } = new List<Source>();

    /// <summary>
    /// get - List of content linked to this license.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    protected License() { }

    public License(string name, int ttl) : base(name)
    {
        this.TTL = ttl;
    }
    #endregion
}
