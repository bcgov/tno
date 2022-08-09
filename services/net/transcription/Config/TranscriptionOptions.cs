
using TNO.Services.Config;

namespace TNO.Services.Transcription.Config;

/// <summary>
/// TranscriptionOptions class, configuration options for transcription service
/// </summary>
public class TranscriptionOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - An array of topics to consume.
    /// </summary>
    public string TranscriptionTopic { get; set; } = "";

    /// <summary>
    /// get/set - The path to clip files.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// get/set - region for speech account.
    /// </summary>
    public string AzureRegion { get; set; } = "";

    /// <summary>
    /// get/set - key for speech account access.
    /// </summary>
    public string AzureCognitiveServicesKey { get; set; } = "";

    #endregion

    #region Methods
    /// <summary>
    /// Get an array of topics.
    /// </summary>
    /// <returns></returns>
    public string[] GetTopics()
    {
        return this.Topics.Split(',').Select(v => v.Trim()).ToArray();
    }
    #endregion
}
