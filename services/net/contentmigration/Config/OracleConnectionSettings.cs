using System.ComponentModel.DataAnnotations;

namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// OracleConnectionSettings class, configuration options for connection to an Oracle database.
/// </summary>
public class OracleConnectionSettings
{
    #region Properties
    /// <summary>
    /// get/set - The Host name of the db server
    /// </summary>
    [Required]
    public string HostName { get; set; } = "";

    /// <summary>
    /// get/set - The Port of the db server
    /// </summary>
    [Required]
    public int? Port { get; set; }

    /// <summary>
    /// get/set - The SID (db name)
    /// </summary>
    [Required]
    public string Sid { get; set; } = "";

    /// <summary>
    /// get/set - The user id to use to connect to the database.
    /// </summary>
    [Required]
    public string UserName { get; set; } = "";

    /// <summary>
    /// get/set - The password to use to connect to the database.
    /// </summary>
    [Required]
    public string Password { get; set; } = "";

    #endregion
}
