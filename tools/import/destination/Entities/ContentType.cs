using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_type")]
public class ContentType : BaseType<int>
{
    #region Properties
    public List<Content> Contents { get; } = new List<Content>();
    public List<Action> Actions { get; } = new List<Action>();
    #endregion

    #region Constructors
    protected ContentType() { }

    public ContentType(string name) : base(name)
    {
    }
    #endregion
}