
using System.Text.Json.Serialization;

namespace TNO.API.Models.Auth;

/// <summary>
/// LocationModel class, provides a model to capture location data.
/// https://geolocation-db.com/json/
/// </summary>
public class LocationModel
{
    #region Properties
    /// <summary>
    /// get/set - A unique key to identify a device.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - IP address
    /// </summary>
    [JsonPropertyName("IPv4")]
    public string IPv4 { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>

    [JsonPropertyName("country_name")]
    public string CountryName { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string? City { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string? State { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string? Postal { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// get/set - The last time this IP was logged.
    /// </summary>
    public DateTime? LastLoginOn { get; set; }
    #endregion
}
