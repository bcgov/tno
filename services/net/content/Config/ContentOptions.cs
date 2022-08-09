
using TNO.Services.Config;

namespace TNO.Services.Content.Config;

/// <summary>
/// ContentOptions class, configuration options for content service
/// </summary>
public class ContentOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of topics to consume.
    /// </summary>
    public string ContentTopics { get; set; } = "";

    /// <summary>
    /// get/set - An array of topics to consume.
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
    #endregion

    #region Methods
    /// <summary>
    /// Get an array of topics.
    /// </summary>
    /// <returns></returns>
    public string[] GetContentTopics()
    {
        return this.ContentTopics.Split(',').Select(v => v.Trim()).ToArray();
    }
    #endregion
}
