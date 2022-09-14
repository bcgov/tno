
using TNO.Services.Config;

namespace TNO.Services.Content.Config;

/// <summary>
/// ContentOptions class, configuration options for content service
/// </summary>
public class ContentOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string ContentTopics { get; set; } = "";

    /// <summary>
    /// get/set - A topic that the Transcription Service subscribes to.
    /// Leave blank if it should not publish messages to the Transcription Service.
    /// </summary>
    public string TranscriptionTopic { get; set; } = "";

    /// <summary>
    /// get/set - The path to capture files.
    /// </summary>
    public string CapturePath { get; set; } = "";

    /// <summary>
    /// get/set - The path to clip files.
    /// </summary>
    public string ClipPath { get; set; } = "";

    /// <summary>
    /// get/set - The number of attempts to retry a failed import.
    /// A retry that ultimately fails will still only count as a single failure for the service.
    /// </summary>
    public int RetryLimit { get; set; } = 3;
    #endregion

    #region Methods
    /// <summary>
    /// Get the configured topics, or return the default topics.
    /// </summary>
    /// <param name="defaultTopics"></param>
    /// <returns></returns>
    public string[] GetContentTopics(string[]? defaultTopics = null)
    {
        var topics = this.ContentTopics?.Split(',').Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
        return topics.Length > 0 ? topics : defaultTopics ?? Array.Empty<string>();
    }
    #endregion
}
