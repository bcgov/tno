using System.Text.Json;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.ContentAnalysis;

/// <summary>
/// ContentAnalysisModel class, a content item's structured analysis.
/// </summary>
public class ContentAnalysisModel
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    public long Id { get; set; }

    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - The input fingerprint analyzed.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - Whether this analysis is of the content's current input.</summary>
    public bool IsCurrent { get; set; }

    /// <summary>get/set - The content had no usable text.</summary>
    public bool IsMetadataOnly { get; set; }

    /// <summary>get/set - The model deployment used.</summary>
    public string Model { get; set; } = "";

    /// <summary>get/set - Versions: normalization, schema, prompt.</summary>
    public string Versions { get; set; } = "";

    /// <summary>get/set - The summary.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - Key facts.</summary>
    public JsonElement KeyFacts { get; set; }

    /// <summary>get/set - People and organizations.</summary>
    public JsonElement Entities { get; set; }

    /// <summary>get/set - Places.</summary>
    public JsonElement Places { get; set; }

    /// <summary>get/set - Topics.</summary>
    public JsonElement Topics { get; set; }

    /// <summary>get/set - The primary topic label.</summary>
    public string? PrimaryTopic { get; set; }

    /// <summary>get/set - The staff topic the primary topic matches, when one does.</summary>
    public string? StaffTopic { get; set; }

    /// <summary>get/set - Suggested tags.</summary>
    public JsonElement SuggestedTags { get; set; }

    /// <summary>get/set - The suggested contributor.</summary>
    public string? SuggestedContributor { get; set; }

    /// <summary>get/set - Reported events.</summary>
    public JsonElement Events { get; set; }

    /// <summary>get/set - Verbatim quotes.</summary>
    public JsonElement Quotes { get; set; }

    /// <summary>get/set - Validation results.</summary>
    public JsonElement Validation { get; set; }

    /// <summary>get/set - Prompt tokens.</summary>
    public int PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens.</summary>
    public int CompletionTokens { get; set; }

    /// <summary>get/set - When it was analyzed.</summary>
    public DateTime AnalyzedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisModel.
    /// </summary>
    public ContentAnalysisModel() { }

    /// <summary>
    /// Creates a new instance of a ContentAnalysisModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    public ContentAnalysisModel(Entities.ContentAnalysis entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.InputHash = entity.InputHash;
        this.IsCurrent = entity.IsCurrent;
        this.IsMetadataOnly = entity.IsMetadataOnly;
        this.Model = entity.Model;
        this.Versions = $"normalization {entity.NormalizationVersion}, schema {entity.SchemaVersion}, prompt {entity.PromptVersion}";
        this.Summary = entity.Summary;
        this.KeyFacts = entity.KeyFacts.RootElement.Clone();
        this.Entities = entity.Entities.RootElement.Clone();
        this.Places = entity.Places.RootElement.Clone();
        this.Topics = entity.Topics.RootElement.Clone();
        this.PrimaryTopic = entity.PrimaryTopic;
        this.StaffTopic = entity.AnalysisTopic?.Topic?.Name;
        this.SuggestedTags = entity.SuggestedTags.RootElement.Clone();
        this.SuggestedContributor = entity.SuggestedContributor;
        this.Events = entity.Events.RootElement.Clone();
        this.Quotes = entity.Quotes.RootElement.Clone();
        this.Validation = entity.Validation.RootElement.Clone();
        this.PromptTokens = entity.PromptTokens;
        this.CompletionTokens = entity.CompletionTokens;
        this.AnalyzedOn = entity.AnalyzedOn;
    }
    #endregion
}

/// <summary>
/// ContentFieldOwnershipModel class, who owns a populated editorial value.
/// </summary>
public class ContentFieldOwnershipModel
{
    /// <summary>get/set - The field (summary, contributor, tag, quote, topic).</summary>
    public string Field { get; set; } = "";

    /// <summary>get/set - The collection item's key, or empty for a scalar field.</summary>
    public string ValueKey { get; set; } = "";

    /// <summary>get/set - Who owns the value.</summary>
    public FieldOwner Owner { get; set; }

    /// <summary>get/set - A person removed the value.</summary>
    public bool IsCleared { get; set; }

    /// <summary>
    /// Creates a new instance of a ContentFieldOwnershipModel.
    /// </summary>
    public ContentFieldOwnershipModel() { }

    /// <summary>
    /// Creates a new instance of a ContentFieldOwnershipModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    public ContentFieldOwnershipModel(ContentFieldOwnership entity)
    {
        this.Field = entity.Field;
        this.ValueKey = entity.ValueKey;
        this.Owner = entity.Owner;
        this.IsCleared = entity.IsCleared;
    }
}

/// <summary>
/// ContentAnalysisDetailsModel class, a content item's analysis, field ownership, and recent runs.
/// </summary>
public class ContentAnalysisDetailsModel
{
    /// <summary>get/set - The current analysis.</summary>
    public ContentAnalysisModel? Analysis { get; set; }

    /// <summary>get/set - Recorded ownership (a value without a record is human-owned).</summary>
    public IEnumerable<ContentFieldOwnershipModel> Ownership { get; set; } = Array.Empty<ContentFieldOwnershipModel>();

    /// <summary>get/set - The outcome of the content's recent analysis requests, newest first.</summary>
    public IEnumerable<TNO.Entities.Models.AnalysisRun> Runs { get; set; } = Array.Empty<TNO.Entities.Models.AnalysisRun>();
}
