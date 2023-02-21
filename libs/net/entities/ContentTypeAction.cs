using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentTypeAction class, provides an entity model for the many-to-many relationship between media type and action.
/// This provides a way to identify which media types can be linked to which actions.
/// </summary>
[Table("content_type_action")]
public class ContentTypeAction : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to content type.
    /// </summary>
    [Column("content_type")]
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to action.
    /// </summary>
    [Column("action_id")]
    public int ActionId { get; set; }

    /// <summary>
    /// get/set - The action.
    /// </summary>
    public virtual Action? Action { get; set; }
    #endregion

    #region Constructors
    protected ContentTypeAction() { }

    public ContentTypeAction(ContentType contentType, int actionId)
    {
        this.ContentType = contentType;
        this.ActionId = actionId;
    }

    public ContentTypeAction(ContentType contentType, Action action)
    {
        this.ContentType = contentType;
        this.ActionId = action?.Id ?? throw new ArgumentNullException(nameof(action));
        this.Action = action;
    }
    #endregion
}
