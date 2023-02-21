using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentCategory class, provides an entity model to link (many-to-many) content with categories.
/// </summary>
[Table("content_category")]
public class ContentCategory : AuditColumns, IEquatable<ContentCategory>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to category.
    /// </summary>
    [Column("category_id")]
    public int CategoryId { get; set; }

    /// <summary>
    /// get/set - The category.
    /// </summary>
    public virtual Category? Category { get; set; }

    /// <summary>
    /// get/set - The score of the category.
    /// </summary>
    [Column("score")]
    public int Score { get; set; }
    #endregion

    #region Constructors
    protected ContentCategory() { }

    public ContentCategory(long contentId, int categoryId, int score)
    {
        this.ContentId = contentId;
        this.CategoryId = categoryId;
        this.Score = score;
    }

    public ContentCategory(Content content, Category category, int score)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.CategoryId = category?.Id ?? throw new ArgumentNullException(nameof(category));
        this.Category = category;
        this.Score = score;
    }
    #endregion

    #region Methods
    public bool Equals(ContentCategory? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.CategoryId == other.CategoryId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentCategory);
    public override int GetHashCode() => (this.ContentId, this.CategoryId).GetHashCode();
    #endregion
}
