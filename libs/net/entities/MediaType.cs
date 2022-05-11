using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("media_types", "lookups")]
[Table("media_type")]
public class MediaType : BaseType<int>
{
    #region Properties
    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();
    public virtual List<Content> Contents { get; set; } = new List<Content>();
    #endregion

    #region Constructors
    protected MediaType() { }

    public MediaType(string name) : base(name)
    {
    }
    #endregion
}
