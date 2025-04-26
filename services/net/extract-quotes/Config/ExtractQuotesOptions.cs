using TNO.Services.Config;

namespace TNO.Services.ExtractQuotes.Config;

/// <summary>
/// Configuration for a specific LLM API endpoint (primary or fallback)
/// </summary>
public class ApiConfig
{
    public List<string> ApiKeys { get; set; } = new List<string>();
    public string ModelName { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60; // Default timeout
    // API URL for this specific endpoint
    public string ApiUrl { get; set; } = string.Empty;
}

/// <summary>
/// Configuration options specifically for LLM usage
/// </summary>
public class LLMOptions
{
    public ApiConfig Primary { get; set; } = new ApiConfig();
    public ApiConfig Fallback { get; set; } = new ApiConfig();
    public int MaxRequestsPerMinute { get; set; } = 10;
}

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
    public bool UseLLM { get; set; } = true;

    /// <summary>
    /// get/set - Ignore any content that was indexed before this day offset.
    /// </summary>
    public int? IgnoreContentPublishedBeforeOffset { get; set; }

    /// <summary>
    /// get/set - Whether to use batch processing for quote extraction.
    /// </summary>
    public bool UseBatchProcessing { get; set; } = true;

    /// <summary>
    /// get/set - Maximum number of content items to process in a single batch.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// get/set - Maximum time in milliseconds to wait before processing a batch even if it's not full.
    /// </summary>
    public int BatchTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Holds the detailed configuration for primary and fallback LLM APIs.
    /// Populated from the "LLM" section in appsettings.json.
    /// </summary>
    public LLMOptions LLM { get; set; } = new LLMOptions();
    #endregion
}
