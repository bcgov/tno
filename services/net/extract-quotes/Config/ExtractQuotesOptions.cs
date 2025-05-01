using TNO.Services.Config;

namespace TNO.Services.ExtractQuotes.Config;

/// <summary>
/// ExtractQuotesOptions class, configuration options for ExtractQuotes service
/// </summary>
public class ExtractQuotesOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Extract quotes when the Index Action is Index?
    /// </summary>
    public bool ExtractQuotesOnIndex { get; set; } = false;

    /// <summary>
    /// get/set - Extract quotes when the Index Action is Publish?
    /// </summary>
    public bool ExtractQuotesOnPublish { get; set; } = true;

    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The URL to the OpenNLP API.
    /// </summary>
    public string CoreNLPApiUrl { get; set; } = "";

    /// <summary>
    /// get/set - Whether to use the LLM API instead of CoreNLP.
    /// </summary>
    public bool UseLLM { get; set; } = false;

    /// <summary>
    /// The required prompt template for quote extraction when UseLLM is true.
    /// Must contain the placeholder '{InputText}' for the text to be processed.
    /// This value MUST be provided in the configuration (appsettings.json) at the Service level.
    /// </summary>
    public string QuoteExtractionPromptTemplate { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Ignore any content that was indexed before this day offset.
    /// </summary>
    public int? IgnoreContentPublishedBeforeOffset { get; set; }

    #region LLM Configuration
    /// <summary>
    /// API keys for the primary LLM model as a semicolon-separated string
    /// </summary>
    public string PrimaryApiKeys { get; set; } = string.Empty;

    /// <summary>
    /// The name of the primary LLM model
    /// </summary>
    public string PrimaryModelName { get; set; } = string.Empty;

    /// <summary>
    /// The API URL for the primary LLM model
    /// </summary>
    public string PrimaryApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// API keys for the fallback LLM model as a semicolon-separated string
    /// </summary>
    public string FallbackApiKeys { get; set; } = string.Empty;

    /// <summary>
    /// The name of the fallback LLM model
    /// </summary>
    public string FallbackModelName { get; set; } = string.Empty;

    /// <summary>
    /// The API URL for the fallback LLM model
    /// </summary>
    public string FallbackApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of LLM API requests per minute
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 10;
    #endregion

    /// <summary>
    /// Get primary API keys as a list 
    /// </summary>
    /// <returns>List of API keys</returns>
    public List<string> GetPrimaryApiKeysList()
    {
        if (string.IsNullOrWhiteSpace(PrimaryApiKeys))
            return new List<string>();

        return PrimaryApiKeys.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    /// <summary>
    /// Get fallback API keys as a list
    /// </summary>
    /// <returns>List of API keys</returns>
    public List<string> GetFallbackApiKeysList()
    {
        if (string.IsNullOrWhiteSpace(FallbackApiKeys))
            return new List<string>();

        return FallbackApiKeys.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
    #endregion
}
