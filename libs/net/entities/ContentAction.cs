using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_action")]
public class ContentAction : AuditColumns, IEquatable<ContentAction>
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("action_id")]
    public int ActionId { get; set; }

    public virtual Action? Action { get; set; }

    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentAction() { }

    public ContentAction(Content content, Action action, string value)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.ActionId = action?.Id ?? throw new ArgumentNullException(nameof(action));
        this.Action = action;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ContentAction(long contentId, int actionId, string value)
    {
        this.ContentId = contentId;
        this.ActionId = actionId;
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion

    #region Methods
    public bool Equals(ContentAction? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.ActionId == other.ActionId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentAction);
    public override int GetHashCode() => (this.ContentId, this.ActionId).GetHashCode();
    #endregion
}
