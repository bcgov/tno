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

    #region Azure Video Indexer configuration
    /// <summary>
    /// get/set - Azure Video Indexer account ID.
    /// </summary>
    public string AzureVideoIndexerAccountId { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure Video Indexer location (e.g., "trial", "canadacentral").
    /// </summary>
    public string AzureVideoIndexerLocation { get; set; } = "trial";

    /// <summary>
    /// get/set - Azure Video Indexer API key (Ocp-Apim-Subscription-Key).
    /// Used for classic API Key authentication (trial/local testing).
    /// </summary>
    public string AzureVideoIndexerApiKey { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Timeout in minutes for Video Indexer processing.
    /// </summary>
    public int AzureVideoIndexerTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// get/set - Polling interval in seconds for Video Indexer status checks.
    /// </summary>
    public int AzureVideoIndexerPollingIntervalSeconds { get; set; } = 30;
    #endregion

    #region Azure Video Indexer ARM Authentication
    /// <summary>
    /// get/set - Azure AD Tenant ID for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerArmTenantId { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure AD Client ID (Application ID) for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerArmClientId { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure AD Client Secret for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerArmClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure Subscription ID for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure Resource Group name for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerResourceGroup { get; set; } = string.Empty;

    /// <summary>
    /// get/set - Azure Video Indexer account name for ARM authentication.
    /// </summary>
    public string AzureVideoIndexerAccountName { get; set; } = string.Empty;

    /// <summary>
    /// Returns true if ARM authentication is fully configured.
    /// When true, ARM auth will be used; otherwise falls back to API Key.
    /// </summary>
    public bool UseArmAuthentication =>
        !string.IsNullOrWhiteSpace(AzureVideoIndexerArmTenantId) &&
        !string.IsNullOrWhiteSpace(AzureVideoIndexerArmClientId) &&
        !string.IsNullOrWhiteSpace(AzureVideoIndexerArmClientSecret);
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
