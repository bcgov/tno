namespace TNO.TemplateEngine.Config;

/// <summary>
/// AzureOptions class, provides a way to configure Azure settings.
/// </summary>
public class AzureOptions
{
    #region Properties
    /// <summary>
    /// get/set - Azure AI configuration settings.
    /// </summary>
    public AzureAIOptions? AI { get; set; }
    #endregion
}
