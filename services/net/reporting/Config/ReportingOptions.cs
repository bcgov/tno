
using TNO.Services.Config;

namespace TNO.Services.Reporting.Config;

/// <summary>
/// ReportingOptions class, configuration options for reporting service
/// </summary>
public class ReportingOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";
    #endregion
}
