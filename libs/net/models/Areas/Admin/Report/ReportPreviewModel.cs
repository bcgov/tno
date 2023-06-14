namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// ReportPreviewModel class, provides a model that represents an report preview.
/// </summary>
public class ReportPreviewModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The body of the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Elasticsearch results.
    /// </summary>
    public object? Results { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportPreviewModel.
    /// </summary>
    public ReportPreviewModel() { }

    /// <summary>
    /// Creates a new instance of an ReportPreviewModel, initializes with specified parameter.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="results"></param>
    public ReportPreviewModel(string subject, string body, object results)
    {
        this.Subject = subject;
        this.Body = body;
        this.Results = results;
    }
    #endregion
}
