using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Category class, provides an entity model to group related content.
/// </summary>
[Cache("categories", "lookups")]
[Table("category")]
public class Category : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of category (issue, proactive).
    /// </summary>
    [Column("category_type")]
    public CategoryType CategoryType { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get - List of content linked to this category.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of many-to-many content linked to this category.
    /// </summary>
    public virtual List<ContentCategory> ContentsManyToMany { get; } = new List<ContentCategory>();
    #endregion

    #region Constructors
    protected Category() { }

    public Category(string name, CategoryType type = CategoryType.Issues) : base(name)
    {
        this.CategoryType = type;
    }
    #endregion
}
