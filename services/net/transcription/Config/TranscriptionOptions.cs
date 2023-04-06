
using TNO.Services.Config;

namespace TNO.Services.Transcription.Config;

/// <summary>
/// TranscriptionOptions class, configuration options for transcription service
/// </summary>
public class TranscriptionOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - region for speech account.
    /// </summary>
    public string AzureRegion { get; set; } = "";

    /// <summary>
    /// get/set - key for speech account access.
    /// </summary>
    public string AzureCognitiveServicesKey { get; set; } = "";

    /// <summary>
    /// get/set - Whether to only accept messages from Kafka that include work orders.
    /// </summary>
    public bool AcceptOnlyWorkOrders { get; set; } = true;
    #endregion
}
