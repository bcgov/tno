using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_category")]
public class ContentCategory : AuditColumns, IEquatable<ContentCategory>
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    [Column("score")]
    public int Score { get; set; }
    #endregion

    #region Constructors
    protected ContentCategory() { }

    public ContentCategory(Content content, Category category, int score)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.CategoryId = category?.Id ?? throw new ArgumentNullException(nameof(category));
        this.Category = category;
        this.Score = score;
    }

    public ContentCategory(long contentId, int categoryId, int score)
    {
        this.ContentId = contentId;
        this.CategoryId = categoryId;
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
