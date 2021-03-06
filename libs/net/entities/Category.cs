using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Category class, provides an entity model used to categorize content.
/// </summary>
[Cache("categories", "lookups")]
[Table("category")]
public class Category : BaseType<int>
{
    #region Properties
    public virtual List<Content> Contents { get; } = new List<Content>();

    public virtual List<ContentCategory> ContentsManyToMany { get; } = new List<ContentCategory>();
    #endregion

    #region Constructors
    protected Category() { }

    public Category(string name) : base(name)
    {
    }
    #endregion
}
