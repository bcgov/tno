
using TNO.Services.Config;

namespace TNO.Services.ExtractQuotes.Config;

/// <summary>
/// ExtractQuotesOptions class, configuration options for ExtractQuotes service
/// </summary>
public class ExtractQuotesOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The URL to the OpenNLP API.
    /// </summary>
    public string OpenNLPApiUrl { get; set; } = "";

    #endregion
}
