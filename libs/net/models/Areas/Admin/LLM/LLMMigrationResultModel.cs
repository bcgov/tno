namespace TNO.API.Areas.Admin.Models.LLM;

/// <summary>
/// LLMMigrationResultModel class, provides a model with the outcome of a migration.
/// </summary>
public class LLMMigrationResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The number of items moved to the target connection.
    /// </summary>
    public int Migrated { get; set; }

    /// <summary>
    /// get/set - The number of report sections whose temperature moved into the target's range.
    /// </summary>
    public int TemperaturesChanged { get; set; }

    /// <summary>
    /// get/set - What still uses the original connection.
    /// </summary>
    public LLMUsageModel Usage { get; set; } = new();
    #endregion
}
