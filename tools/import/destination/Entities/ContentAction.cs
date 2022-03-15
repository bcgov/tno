using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_action")]
public class ContentAction : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

    [Column("action_id")]
    public int ActionId { get; set; }

    public Action? Action { get; set; }

    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentAction() { }

    public ContentAction(Content content, Action category, string value)
    {
        if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(value));

        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.ActionId = category?.Id ?? throw new ArgumentNullException(nameof(category));
        this.Action = category;
        this.Value = value;
    }

    public ContentAction(int contentId, int categoryId, string value)
    {
        if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(value));

        this.ContentId = contentId;
        this.ActionId = categoryId;
        this.Value = value;
    }
    #endregion
}