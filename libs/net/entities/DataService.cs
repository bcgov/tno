using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// DataService class, provides a way to desribe and store a data source service.
/// </summary>
[Table("data_service")]
public class DataService
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify data service.  Foreign key to the data source.
    /// </summary>
    [Key]
    [Column("date_source_id")]
    public int DataSourceId { get; set; }

    /// <summary>
    /// get/set - The data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    /// <summary>
    /// get/set - When the data service was ingested last.
    /// </summary>
    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set - Number of sequential failures that have occurred.
    /// </summary>
    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }
    #endregion

    #region Constructors
    protected DataService() { }

    public DataService(DataSource dataSource)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
    }

    public DataService(int dataSourceId)
    {
        this.DataSourceId = dataSourceId;
    }
    #endregion
}
