using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_category")]
public class ContentCategory : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

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

    public ContentCategory(int contentId, int categoryId, int score)
    {
        this.ContentId = contentId;
        this.CategoryId = categoryId;
        this.Score = score;
    }
    #endregion
}