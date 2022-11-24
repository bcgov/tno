using Newtonsoft.Json;

namespace TNO.CSS.API.Models;

/// <summary>
/// RequestTokenFormModel class, provides a model that represents the token request form.
/// </summary>
public class RequestTokenFormModel
{
    #region Properties
    /// <summary>
    /// get/set - The type of grant requested.
    /// </summary>
    [JsonProperty("grant_type")]
    public string GrantType { get; set; } = "client_credentials";
    #endregion
}
