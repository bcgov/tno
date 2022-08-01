using TNO.Services.Config;

namespace TNO.Services.Filemonitor.Config;

/// <summary>
/// FilemonitorOptions class, configuration options for syndication
/// </summary>
public class FilemonitorOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The root path from which to import files.
    /// </summary>
    public string ImportRoot { get; set; } = "";
    #endregion
}
