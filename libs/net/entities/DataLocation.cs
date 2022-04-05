using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("data_location")]
public class DataLocation : BaseType<int>
{
    #region Properties
    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();
    #endregion

    #region Constructors
    protected DataLocation() { }

    public DataLocation(string name) : base(name)
    {
    }
    #endregion
}
