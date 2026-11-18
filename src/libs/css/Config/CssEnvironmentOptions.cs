using System.ComponentModel.DataAnnotations;

namespace TNO.CSS.Config;

public class CssEnvironmentOptions : CssOptions
{
    #region Properties
    /// <summary>
    /// get/set - The CSS integration Id.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'IntegrationId' is required.")]
    public int IntegrationId { get; set; }

    /// <summary>
    /// get/set - The CSS environment.
    /// </summary>
    [Required(ErrorMessage = "Configuration 'Environment' is required.")]
    public string Environment { get; set; } = "";
    #endregion
}
