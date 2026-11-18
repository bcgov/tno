using TNO.AI.Synthesis;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportAISectionTestOutcome record, the outcome of running an AI section once without storing it.
/// </summary>
/// <param name="IsSuccess">Whether synthesis completed.</param>
/// <param name="Output">The section HTML.</param>
/// <param name="Error">Why it failed.</param>
/// <param name="Usage">What it cost, when synthesis ran.</param>
/// <param name="Transcript">Every request sent with its response, in order.</param>
public record ReportAISectionTestOutcome(bool IsSuccess, string? Output, string? Error, SynthesisUsage? Usage, IReadOnlyList<LlmExchange>? Transcript);
