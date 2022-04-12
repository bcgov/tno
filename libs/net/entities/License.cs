using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("license")]
public class License : BaseType<int>
{
    #region Properties
    [Column("ttl")]
    public int TTL { get; set; }

    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();

    public virtual List<Content> Contents { get; set; } = new List<Content>();
    #endregion

    #region Constructors
    protected License() { }

    public License(string name, int ttl) : base(name)
    {
        this.TTL = ttl;
    }
    #endregion
}
