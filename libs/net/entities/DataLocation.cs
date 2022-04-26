using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// DataLocation class, provides a way to identify and describe a location where data will be uploaded and stored.
/// Note this is not the same thing as the data source location.
/// </summary>
[Table("data_location")]
public class DataLocation : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The location type.
    /// </summary>
    [Column("location_type")]
    public DataLocationType LocationType { get; set; }

    /// <summary>
    /// get/set - Connection settings for the data location.
    /// </summary>
    [Column("connection")]
    public string Connection { get; set; } = "{}";

    /// <summary>
    /// get - Collection of data sources that use this location.
    /// </summary>
    public virtual List<DataSource> DataSources { get; } = new List<DataSource>();
    #endregion

    #region Constructors
    protected DataLocation() { }

    public DataLocation(string name) : base(name)
    {
    }
    #endregion
}
