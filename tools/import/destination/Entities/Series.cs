using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("series")]
public class Series : BaseType<int>
{
    #region Properties

    public List<Content> Contents { get; set; } = new List<Content>();
    #endregion

    #region Constructors
    protected Series() { }

    public Series(string name) : base(name)
    {
    }
    #endregion
}