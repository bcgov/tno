using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("media_type")]
public class MediaType : BaseType<int>
{
    #region Properties
    public List<DataSource> DataSources { get; set; } = new List<DataSource>();
    public List<Content> Contents { get; set; } = new List<Content>();
    #endregion

    #region Constructors
    protected MediaType() { }

    public MediaType(string name) : base(name)
    {
    }
    #endregion
}