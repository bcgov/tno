namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportResultModel class, provides a model that represents an report preview result.
/// </summary>
public class ReportResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The report primary key.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The report instance primary key.
    /// </summary>
    public long? InstanceId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The body of the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - JSON data that was used to generate the report.
    /// </summary>
    public object? Data { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportResultModel.
    /// </summary>
    public ReportResultModel() { }

    /// <summary>
    /// Creates a new instance of an ReportResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="data"></param>
    public ReportResultModel(Entities.ReportInstance instance, object? data = null)
    {
        this.ReportId = instance.ReportId;
        this.InstanceId = instance.Id;
        this.Subject = instance.Subject;
        this.Body = instance.Body;
        this.Data = data;
    }
    #endregion
}
