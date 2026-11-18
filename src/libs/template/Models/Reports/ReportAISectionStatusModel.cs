namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportAISectionStatusModel class, provides a model that represents the state of an AI section in a report preview.
/// </summary>
public class ReportAISectionStatusModel
{
    #region Properties
    /// <summary>
    /// get/set - The report section primary key.
    /// </summary>
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The section name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The section label.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// get/set - The state of the section.
    /// </summary>
    public AISectionStatus Status { get; set; }

    /// <summary>
    /// get/set - Why the section could not be generated.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// get/set - When the generator holding the section is considered to have stopped, if it has not stored a result.
    /// </summary>
    public DateTime? ExpiresOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportAISectionStatusModel.
    /// </summary>
    public ReportAISectionStatusModel() { }

    /// <summary>
    /// Creates a new instance of an ReportAISectionStatusModel, initializes with specified parameter.
    /// </summary>
    /// <param name="section"></param>
    public ReportAISectionStatusModel(ReportSectionModel section)
    {
        this.SectionId = section.Id;
        this.Name = section.Name;
        this.Label = section.Settings.Label;
        this.Status = section.AIStatus ?? AISectionStatus.Ready;
        this.Error = section.AIError;
        this.ExpiresOn = section.AIExpiresOn;
    }
    #endregion
}
