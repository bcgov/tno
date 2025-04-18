
using TNO.Kafka.Models;
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
    /// get/set - The URL to the LLM API (compatible with OpenAI format).
    /// </summary>
    public string LLMApiUrl { get; set; } = "";

    /// <summary>
    /// get/set - The API key for the LLM API.
    /// </summary>
    public string LLMApiKey { get; set; } = "";

    /// <summary>
    /// get/set - The model name to use for the LLM API.
    /// </summary>
    public string LLMModelName { get; set; } = "gpt-3.5-turbo";

    /// <summary>
    /// get/set - Whether to use the LLM API instead of CoreNLP.
    /// </summary>
    public bool UseLLM { get; set; } = true;

    /// <summary>
    /// get/set - Maximum number of LLM API requests per minute.
    /// </summary>
    public int LLMMaxRequestsPerMinute { get; set; } = 10;

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
    #endregion
}
