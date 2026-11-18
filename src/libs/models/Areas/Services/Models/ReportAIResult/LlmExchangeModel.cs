namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// LlmExchangeModel class, one request an AI section sent (or would send) to its model and what
/// came back. A planned exchange has no response.
/// </summary>
public class LlmExchangeModel
{
    #region Properties
    /// <summary>get/set - The display label of the step, e.g. "Map batch 2/5", "Reduce round 1", "Final".</summary>
    public string Stage { get; set; } = "";

    /// <summary>get/set - The kind of step: Map, Reduce, Final, HistoryMap, HistoryReduce or Agent.</summary>
    public string Kind { get; set; } = "";

    /// <summary>get/set - The send order, from 1.</summary>
    public int Sequence { get; set; }

    /// <summary>get/set - The messages sent.</summary>
    public LlmMessageModel[] Messages { get; set; } = Array.Empty<LlmMessageModel>();

    /// <summary>get/set - The response text, or null when planned or failed.</summary>
    public string? Response { get; set; }

    /// <summary>get/set - Prompt tokens the provider reported.</summary>
    public int? PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens the provider reported.</summary>
    public int? CompletionTokens { get; set; }

    /// <summary>get/set - How long the request took.</summary>
    public long DurationMs { get; set; }

    /// <summary>get/set - Whether structured JSON output was requested.</summary>
    public bool JsonMode { get; set; }

    /// <summary>get/set - Whether the response stopped at the output limit.</summary>
    public bool IsTruncated { get; set; }

    /// <summary>get/set - Why the request failed, when it did.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - The request's estimated size in tokens before it was sent.</summary>
    public int EstimatedTokens { get; set; }
    #endregion
}
