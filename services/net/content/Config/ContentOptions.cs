
using TNO.Services.Config;

namespace TNO.Services.Content.Config;

/// <summary>
/// ContentOptions class, configuration options for Content
/// </summary>
public class ContentOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of topics to consume.
    /// </summary>
    public string[] Topics { get; set; } = Array.Empty<string>();
    #endregion
}
