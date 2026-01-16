using System.ComponentModel.DataAnnotations;
using TNO.Services.Config;

namespace TNO.Services.AutoClipper.Config;

public class AutoClipperOptions : ServiceOptions
{
    /// <summary>
    /// get/set - The topic name in Kafka to subscriber for work orders.
    /// </summary>
    public string Topics { get; set; } = "request-clips";

    /// <summary>
    /// get/set - The path to the mapped volume containing the files.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - An array of tag codes to add to content when it is created.
    /// </summary>
    public string[] ApplyTags { get; set; } = [];

    /// <summary>
    /// get/set - Path to the location station configuration files are stored.
    /// </summary>
    public string StationConfigPath { get; set; } = Path.Combine("Config", "Stations");

    /// <summary>
    /// get/set - The maximum number of stories to generate from a clip.
    /// </summary>
    public int MaxStoriesFromClip { get; set; } = 5;

    #region Azure Speech Service configuration
    /// <summary>
    /// get/set - The API key to use Azure Speech services.
    /// </summary>
    public string AzureSpeechKey { get; set; } = "";

    /// <summary>
    /// get/set - The region the Azure speech services is deployed.
    /// </summary>
    public string AzureSpeechRegion { get; set; } = "";

    /// <summary>
    /// get/set - The URL endpoint to the batch service.
    /// </summary>
    public string AzureSpeechBatchEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// get/set - The API version.
    /// </summary>
    public string AzureSpeechBatchApiVersion { get; set; } = "v3.2";

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public int AzureSpeechBatchPollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public int AzureSpeechBatchTimeoutMinutes { get; set; } = 45;

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public string DefaultTranscriptLanguage { get; set; } = "en-US";

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public int AzureSpeechMaxRetries { get; set; } = 3;

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public int AzureSpeechRetryDelaySeconds { get; set; } = 5;
    #endregion

    #region Azure Storage configuration
    /// <summary>
    /// get/set - Configure Azure storage connection string.
    /// </summary>
    public string AzureSpeechStorageConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Configure Azure speech services.
    /// </summary>
    public string AzureSpeechStorageContainer { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Configure Azure storage services.
    /// </summary>
    public int AzureSpeechStorageSasExpiryMinutes { get; set; } = 180;
    #endregion

    #region Azure AI configuration
    /// <summary>
    /// get/set - The URL to the LLM
    /// </summary>
    [Required]
    public Uri LlmApiUrl { get; set; } = default!;

    /// <summary>
    /// get/set - The API key
    /// </summary>
    [Required]
    public string LlmApiKey { get; set; } = "";

    /// <summary>
    /// get/set - The LLM model to use.
    /// </summary>
    public string LlmDefaultModel { get; set; } = "";

    /// <summary>
    /// get/set - The Default LLM prompt.
    /// </summary>
    public string LlmPrompt { get; set; } = string.Empty;

    /// <summary>
    /// get/set - The maximum prompt character limit.
    /// </summary>
    public int LlmPromptCharacterLimit { get; set; } = 0;

    /// <summary>
    /// get/set - The LLM boundary score threshold.
    /// </summary>
    public double LlmBoundaryScoreThreshold { get; set; } = 0.55;
    #endregion

}
