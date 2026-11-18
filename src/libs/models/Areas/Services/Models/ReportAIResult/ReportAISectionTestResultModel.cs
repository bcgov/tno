using System.Text.Json;

namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAISectionTestResultModel class, the outcome of a test run of an AI section: its output,
/// what it cost, and every request sent with its response.
/// </summary>
public class ReportAISectionTestResultModel
{
    #region Properties
    /// <summary>get/set - Identifies the run.</summary>
    public Guid TestId { get; set; }

    /// <summary>get/set - The report.</summary>
    public int ReportId { get; set; }

    /// <summary>get/set - The report instance whose content was used.</summary>
    public long ReportInstanceId { get; set; }

    /// <summary>get/set - The AI section.</summary>
    public int SectionId { get; set; }

    /// <summary>get/set - The user who requested the run and is told when it completes.</summary>
    public int? RequestorId { get; set; }

    /// <summary>get/set - The state of the run.</summary>
    public ReportAISectionTestStatus Status { get; set; }

    /// <summary>get/set - The section HTML.</summary>
    public string? Output { get; set; }

    /// <summary>get/set - Why the run failed.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - Requests sent to the model.</summary>
    public int RequestCount { get; set; }

    /// <summary>get/set - Prompt tokens reported by the provider.</summary>
    public int PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens reported by the provider.</summary>
    public int CompletionTokens { get; set; }

    /// <summary>get/set - Generation time in milliseconds.</summary>
    public long DurationMs { get; set; }

    /// <summary>get/set - Stories whose text was sent to the model.</summary>
    public int StoryCount { get; set; }

    /// <summary>get/set - The deepest reduction round.</summary>
    public int ReductionDepth { get; set; }

    /// <summary>get/set - Every request sent, in order, with its response.</summary>
    public LlmExchangeModel[] Transcript { get; set; } = Array.Empty<LlmExchangeModel>();

    /// <summary>get/set - When the run was requested.</summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>get/set - When the run finished.</summary>
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAISectionTestResultModel object.
    /// </summary>
    public ReportAISectionTestResultModel() { }

    /// <summary>
    /// Creates a new instance of a ReportAISectionTestResultModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options">Serializer options the transcript was stored with.</param>
    public ReportAISectionTestResultModel(Entities.ReportAITest entity, JsonSerializerOptions options)
    {
        this.TestId = entity.Id;
        this.ReportId = entity.ReportId;
        this.ReportInstanceId = entity.ReportInstanceId;
        this.SectionId = entity.ReportSectionId;
        this.RequestorId = entity.RequestorId;
        this.Status = (ReportAISectionTestStatus)(int)entity.Status;
        this.Output = entity.Output;
        this.Error = entity.Error;
        this.RequestCount = entity.RequestCount;
        this.PromptTokens = entity.PromptTokens;
        this.CompletionTokens = entity.CompletionTokens;
        this.DurationMs = entity.DurationMs;
        this.StoryCount = entity.StoryCount;
        this.ReductionDepth = entity.ReductionDepth;
        this.Transcript = ParseTranscript(entity.Transcript, options);
        this.CreatedOn = entity.CreatedOn;
        this.CompletedOn = entity.CompletedOn;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Copy this outcome into a DB entity.
    /// </summary>
    /// <param name="options">Serializer options to store the transcript with.</param>
    /// <returns></returns>
    public Entities.ReportAITest ToEntity(JsonSerializerOptions options)
    {
        return new Entities.ReportAITest(this.TestId, this.ReportId, this.ReportInstanceId, this.SectionId, this.RequestorId ?? 0)
        {
            Status = (Entities.ReportAITestStatus)(int)this.Status,
            Output = this.Output,
            Error = this.Error,
            RequestCount = this.RequestCount,
            PromptTokens = this.PromptTokens,
            CompletionTokens = this.CompletionTokens,
            DurationMs = this.DurationMs,
            StoryCount = this.StoryCount,
            ReductionDepth = this.ReductionDepth,
            Transcript = JsonSerializer.Serialize(this.Transcript, options),
            CreatedOn = this.CreatedOn,
            CompletedOn = this.CompletedOn,
        };
    }

    private static LlmExchangeModel[] ParseTranscript(string? json, JsonSerializerOptions options)
    {
        if (String.IsNullOrWhiteSpace(json)) return Array.Empty<LlmExchangeModel>();
        try
        {
            return JsonSerializer.Deserialize<LlmExchangeModel[]>(json, options) ?? Array.Empty<LlmExchangeModel>();
        }
        catch (JsonException)
        {
            return Array.Empty<LlmExchangeModel>();
        }
    }
    #endregion
}
