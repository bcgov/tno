using System.Text.Json.Serialization;

namespace TNO.Elastic.Migration.Models;

/// <summary>
/// MigrationVersion class, provides a model to serialize migration version information.
/// </summary>
public class MigrationVersion
{
    #region Properties
    /// <summary>
    /// get/set - The version name.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    /// <summary>
    /// get/set - A description of the migration.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationVersion object.
    /// </summary>
    public MigrationVersion() { }

    /// <summary>
    /// Creates a new instance of a MigrationVersion object, initializes with specified parameters.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="description"></param>
    public MigrationVersion(string version, string description)
    {
        this.Version = version;
        this.Description = description;
    }
    #endregion
}
