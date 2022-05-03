using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("series")]
[Table("series")]
public class Series : BaseType<int>
{
    #region Properties

    public virtual List<Content> Contents { get; set; } = new List<Content>();
    #endregion

    #region Constructors
    protected Series() { }

    public Series(string name) : base(name)
    {
    }
    #endregion
}
