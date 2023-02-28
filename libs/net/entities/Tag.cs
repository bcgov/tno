using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Tag class, provides an entity model to filter content.
/// </summary>
[Cache("tags", "lookups")]
[Table("tag")]
public class Tag : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Unique code to identify the entity.
    /// </summary>
    [Column("code")]
    public string Code { get; set; }
    
    /// <summary>
    /// get - List of content with this tag.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of content with this tag (many-to-many relationship).
    /// </summary>
    public virtual List<ContentTag> ContentsManyToMany { get; } = new List<ContentTag>();
    #endregion

    #region Constructors
    protected Tag(string code) { 
        this.Code = code;
    }

    public Tag(int id, string name, string code) : base(id, name)
    {
        this.Code = code;
    }
    #endregion
}
