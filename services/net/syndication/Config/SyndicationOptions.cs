using TNO.Services.Config;

namespace TNO.Services.Syndication.Config;

/// <summary>
/// SyndicationOptions class, configuration options for syndication
/// </summary>
public class SyndicationOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The invalid characters and their expected format so this could be cleaned up 
    /// before importing, and it should look like this - "{key1}:_{value1}_{value2}__{key2}:_{value3}"
    /// </summary>
    public string InvalidEncodings { get; set; } = "";

    /// <summary>
    /// get - The key value set from InvalidEncodings ["{key1}:_{value1}_{value2}", "{key2}:_{value3}"]
    /// </summary>    
    public string[]? EncodingSets => InvalidEncodings?.Split("__", StringSplitOptions.RemoveEmptyEntries);
    #endregion
}
