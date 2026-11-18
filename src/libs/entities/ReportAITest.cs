using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ReportAITest class, provides a DB model for one test run of an AI report section, requested
/// from the prompt workbench. The browser-facing API starts the row, the reporting service posts
/// the outcome through the services API, and the row is short-lived: it is replaced by the
/// requestor's next run of the section, deleted when the workbench is left, and purged after an hour.
/// </summary>
[Table("report_ai_test")]
public class ReportAITest : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key, the test id the browser polls for.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report.
    /// </summary>
    [Column("report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The report.
    /// </summary>
    public virtual Report? Report { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report instance whose content the run read.
    /// </summary>
    [Column("report_instance_id")]
    public long ReportInstanceId { get; set; }

    /// <summary>
    /// get/set - The report instance.
    /// </summary>
    public virtual ReportInstance? ReportInstance { get; set; }

    /// <summary>
    /// get/set - Foreign key to the AI section.
    /// </summary>
    [Column("report_section_id")]
    public int ReportSectionId { get; set; }

    /// <summary>
    /// get/set - The AI section.
    /// </summary>
    public virtual ReportSection? ReportSection { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user who requested the run.
    /// </summary>
    [Column("requestor_id")]
    public int RequestorId { get; set; }

    /// <summary>
    /// get/set - The user who requested the run.
    /// </summary>
    public virtual User? Requestor { get; set; }

    /// <summary>
    /// get/set - The state of the run.
    /// </summary>
    [Column("status")]
    public ReportAITestStatus Status { get; set; }

    /// <summary>
    /// get/set - The generated section HTML.
    /// </summary>
    [Column("output")]
    public string? Output { get; set; }

    /// <summary>
    /// get/set - Why the run failed.
    /// </summary>
    [Column("error")]
    public string? Error { get; set; }

    /// <summary>
    /// get/set - Requests sent to the model.
    /// </summary>
    [Column("request_count")]
    public int RequestCount { get; set; }

    /// <summary>
    /// get/set - Prompt tokens reported by the provider.
    /// </summary>
    [Column("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// get/set - Completion tokens reported by the provider.
    /// </summary>
    [Column("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// get/set - Generation time in milliseconds.
    /// </summary>
    [Column("duration_ms")]
    public long DurationMs { get; set; }

    /// <summary>
    /// get/set - Stories whose text was sent to the model.
    /// </summary>
    [Column("story_count")]
    public int StoryCount { get; set; }

    /// <summary>
    /// get/set - The deepest reduction round.
    /// </summary>
    [Column("reduction_depth")]
    public int ReductionDepth { get; set; }

    /// <summary>
    /// get/set - Every request and response of the run, as a JSON array of exchanges.
    /// </summary>
    [Column("transcript")]
    public string Transcript { get; set; } = "[]";

    /// <summary>
    /// get/set - When the outcome was posted.
    /// </summary>
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAITest object.
    /// </summary>
    protected ReportAITest() { }

    /// <summary>
    /// Creates a new instance of a ReportAITest object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="reportId"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="reportSectionId"></param>
    /// <param name="requestorId"></param>
    public ReportAITest(Guid id, int reportId, long reportInstanceId, int reportSectionId, int requestorId)
    {
        this.Id = id;
        this.ReportId = reportId;
        this.ReportInstanceId = reportInstanceId;
        this.ReportSectionId = reportSectionId;
        this.RequestorId = requestorId;
        this.Status = ReportAITestStatus.Pending;
    }
    #endregion
}
