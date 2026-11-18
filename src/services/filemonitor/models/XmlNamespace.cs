using System.Text.Json.Serialization;

namespace TNO.Services.FileMonitor;

/// <summary>
/// Namespace class for XML parsing.
/// </summary>
public class XmlNamespace
{
    #region Properties
    /// <summary>
    /// get/set - The namespace identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// get/set - The URL path to the namespace.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = "";
    #endregion
}
