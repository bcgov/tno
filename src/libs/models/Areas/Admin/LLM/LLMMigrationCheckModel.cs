namespace TNO.API.Areas.Admin.Models.LLM;

/// <summary>
/// LLMMigrationCheckModel class, provides a model with what stops or affects a migration.
/// </summary>
public class LLMMigrationCheckModel
{
    #region Properties
    /// <summary>
    /// get/set - Why the migration cannot run.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - What an administrator should know before running the migration.
    /// </summary>
    public IEnumerable<string> Warnings { get; set; } = Array.Empty<string>();
    #endregion
}
