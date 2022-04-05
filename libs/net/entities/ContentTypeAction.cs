using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_type_action")]
public class ContentTypeAction : AuditColumns
{
    #region Properties
    [Column("content_type_id")]
    public int ContentTypeId { get; set; }

    public virtual ContentType? ContentType { get; set; }

    [Column("action_id")]
    public int ActionId { get; set; }

    public virtual Action? Action { get; set; }
    #endregion

    #region Constructors
    protected ContentTypeAction() { }

    public ContentTypeAction(ContentType contentType, Action action)
    {
        this.ContentTypeId = contentType?.Id ?? throw new ArgumentNullException(nameof(contentType));
        this.ContentType = contentType;
        this.ActionId = action?.Id ?? throw new ArgumentNullException(nameof(action));
        this.Action = action;
    }

    public ContentTypeAction(int contentTypeId, int actionId)
    {
        this.ContentTypeId = contentTypeId;
        this.ActionId = actionId;
    }
    #endregion
}
