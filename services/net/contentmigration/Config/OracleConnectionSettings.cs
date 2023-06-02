using System.ComponentModel.DataAnnotations;

namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// OracleConnectionSettings class, configuration options for connection to an Oracle database.
/// </summary>
public class OracleConnectionSettings
{
    #region Properties
    /// <summary>
    /// get/set - Should only contain the 'Data Source' section of the connection string. For example "Data Source=localhost:41521/freepdb1"
    /// </summary>
    [Required]
    public string DataSource { get; set; } = "";
    /// <summary>
    /// get/set - The user id to use to connect to the database.
    /// </summary>
    [Required]
    public string UserId { get; set; } = "";
    /// <summary>
    /// get/set - The password to use to connect to the database.
    /// </summary>
    [Required]
    public string Password { get; set; } = "";

    #endregion
}
