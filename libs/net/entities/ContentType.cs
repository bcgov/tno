using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_type")]
public class ContentType : BaseType<int>
{
    #region Properties
    public virtual List<Content> Contents { get; } = new List<Content>();
    public virtual List<Action> Actions { get; } = new List<Action>();
    public virtual List<ContentTypeAction> ActionsManyToMany { get; } = new List<ContentTypeAction>();
    #endregion

    #region Constructors
    protected ContentType() { }

    public ContentType(string name) : base(name)
    {
    }
    #endregion
}
