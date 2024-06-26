

namespace TNO.Services.ContentMigration;

/// <summary>
/// ImportMigrationType enum, import options.
/// </summary>
public enum ImportMigrationType
{
    /// <summary>
    /// Migration is not configured.
    /// </summary>
    Unknown,
    /// <summary>
    /// Historical content.
    /// </summary>
    Historic,
    /// <summary>
    /// Any content - This is very slow.
    /// </summary>
    All,
    /// <summary>
    /// Recent content.
    /// </summary>
    Recent,
    /// <summary>
    /// Current content.
    /// </summary>
    Current
}
