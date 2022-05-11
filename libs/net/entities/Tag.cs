using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("tags", "lookups")]
[Table("tag")]
public class Tag : BaseType<string>
{
    #region Properties
    public virtual List<Content> Contents { get; } = new List<Content>();
    public virtual List<ContentTag> ContentsManyToMany { get; } = new List<ContentTag>();
    #endregion

    #region Constructors
    protected Tag() { }

    public Tag(string id, string name) : base(id, name)
    {
    }
    #endregion
}
