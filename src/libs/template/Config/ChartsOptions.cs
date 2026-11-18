namespace TNO.TemplateEngine.Config;

/// <summary>
/// ChartsOptions class, provides a way to configure Charts API.
/// </summary>
public class ChartsOptions
{
    #region Properties
    /// <summary>
    /// get/set - The URL to the Charts API.
    /// </summary>
    public Uri Url { get; set; } = new Uri("http://charts:8080");

    /// <summary>
    /// get/set - Path to the base64 endpoint.
    /// </summary>
    public string Base64Path { get; set; } = "/base64";

    /// <summary>
    /// get/set - Path to the image endpoint.
    /// </summary>
    public string ImagePath { get; set; } = "/image";
    #endregion
}
