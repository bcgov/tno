using System.Text.Json;

namespace TNO.Services.Reporting.Models;

/// <summary>
/// ReportEmailResponseModel class, provides a model to keep CHES email responses.
/// </summary>
public class ReportEmailResponseModel
{
    #region Properties
    /// <summary>
    /// get/set - The CHES response for the email that only contain a link to the website.
    /// </summary>
    public JsonDocument? LinkOnlyFormatResponse { get; set; }

    /// <summary>
    /// get/set - The CHES response for the email that contain the full text report.
    /// </summary>
    public JsonDocument? FullTextFormatResponse { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEmailResponseModel.
    /// </summary>
    public ReportEmailResponseModel()
    {
    }
    #endregion
}
