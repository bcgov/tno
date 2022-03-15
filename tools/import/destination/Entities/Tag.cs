using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("tag")]
public class Tag : BaseType<string>
{
    #region Properties
    public List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    protected Tag() { }

    public Tag(string id, string name) : base(id, name)
    {
    }
    #endregion
}