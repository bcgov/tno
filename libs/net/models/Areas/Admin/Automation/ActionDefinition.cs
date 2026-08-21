using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ActionDefinition class, one action within a step. The action's Type selects its handler;
/// the handler's descriptor (see <see cref="ActionCatalog"/>) declares which of the optional
/// fields below it reads. Whether an action runs is decided by its property condition (When),
/// its LLM gate (Confirm against a raw analysis response, or When.From against a parsed one),
/// or unconditionally when neither is set.
/// </summary>
public class ActionDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The action type (e.g. 'content.update', 'search', 'exclude').
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// get/set - Optional display name (defaults to the type).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// get/set - Whether the action executes.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Property condition / analysis-result gate. Evaluated before any prompt is sent;
    /// a failing condition costs no LLM call.
    /// </summary>
    public ConditionDefinition? When { get; set; }

    /// <summary>
    /// get/set - Confirmation statement matched against a raw analysis response (supports the
    /// '{value}' capture token). Used with raw-mode analyses and migrated v1 actions.
    /// </summary>
    public string? Confirm { get; set; }

    /// <summary>
    /// get/set - The name of the analysis whose response Confirm is matched against
    /// (defaults to the step's only analysis when it has exactly one).
    /// </summary>
    public string? Analysis { get; set; }

    /// <summary>
    /// get/set - The value the action applies, from a fixed source: an analysis result,
    /// a working-copy field, a literal, or a token template.
    /// </summary>
    public ValueSource? Value { get; set; }

    /// <summary>
    /// get/set - The content field written by 'content.update'.
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// get/set - The target of a content action: omitted for the step's subject, or a draft
    /// name (e.g. '$item.digest') created earlier in the same iteration.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// get/set - The reason recorded when 'exclude' removes an item from later steps.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// get/set - The filter executed by 'search'.
    /// </summary>
    public int? Filter { get; set; }

    /// <summary>
    /// get/set - The collection written by 'search', 'collection.create', 'collection.move',
    /// 'collection.add' and 'select-top' (e.g. '$run.digests').
    /// </summary>
    public string? Into { get; set; }

    /// <summary>
    /// get/set - The source collection of a collection operation (remove/move/filter/sort/take/
    /// distinct/union/except/intersect).
    /// </summary>
    [JsonPropertyName("from")]
    public string? FromCollection { get; set; }

    /// <summary>
    /// get/set - The second operand collection of 'collection.union'/'except'/'intersect'.
    /// </summary>
    public string? With { get; set; }

    /// <summary>
    /// get/set - The item reference of collection add/remove/move: '$item' (the subject) or a
    /// draft name (e.g. '$item.digest').
    /// </summary>
    public string? Item { get; set; }

    /// <summary>
    /// get/set - The digest field used by 'collection.sortBy'/'collection.distinctBy'.
    /// </summary>
    public string? By { get; set; }

    /// <summary>
    /// get/set - Sort direction for 'collection.sortBy' ('asc' default, or 'desc').
    /// </summary>
    public string? Direction { get; set; }

    /// <summary>
    /// get/set - The condition applied by 'collection.filter' (items failing it are removed).
    /// </summary>
    public ConditionDefinition? Where { get; set; }

    /// <summary>
    /// get/set - The item count kept by 'collection.take'.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// get/set - The digest field projection of 'search'.
    /// </summary>
    public List<string>? Fields { get; set; }

    /// <summary>
    /// get/set - Maximum items fetched by 'search'.
    /// </summary>
    public int? Max { get; set; }

    /// <summary>
    /// get/set - Per-field character caps applied by 'search' on ingest (e.g. { "body": 2000 }).
    /// </summary>
    public Dictionary<string, int>? Truncate { get; set; }

    /// <summary>
    /// get/set - The collection 'dedupe' compares against.
    /// </summary>
    public string? Against { get; set; }

    /// <summary>
    /// get/set - Dedupe comparison mode: 'iterate' (one prompt per candidate) or 'batch'.
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// get/set - Candidates per prompt in dedupe batch mode.
    /// </summary>
    public int? BatchSize { get; set; }

    /// <summary>
    /// get/set - Cap on candidates examined per item by 'dedupe' (0/null = unbounded).
    /// </summary>
    public int? MaxComparisons { get; set; }

    /// <summary>
    /// get/set - 'dedupe': persist confirmed duplicates as content_link records (value
    /// 'duplicate') and skip the LLM for items already linked.
    /// </summary>
    public bool? Remember { get; set; }

    /// <summary>
    /// get/set - 'content.contributor': create the contributor when no enabled record matches
    /// the value (real runs only; dry runs log the intent).
    /// </summary>
    public bool? Create { get; set; }

    /// <summary>
    /// get/set - Prompt override for 'dedupe' comparisons.
    /// </summary>
    public PromptDefinition? Prompt { get; set; }

    /// <summary>
    /// get/set - The scoring objective of 'score'/'select-top' (e.g. 'top-story').
    /// </summary>
    public string? Objective { get; set; }

    /// <summary>
    /// get/set - How many top-scored items 'select-top' takes.
    /// </summary>
    public int? Take { get; set; }

    /// <summary>
    /// get/set - The content action (e.g. Top Story) applied by 'content.action'/'select-top'.
    /// </summary>
    public int? ContentAction { get; set; }

    /// <summary>
    /// get/set - The report published by 'report.run'.
    /// </summary>
    public int? Report { get; set; }

    /// <summary>
    /// get/set - The notification published by 'notification.run'.
    /// </summary>
    public int? Notification { get; set; }

    /// <summary>
    /// get/set - The collection a report/notification runs against (recorded in the summary).
    /// </summary>
    public string? Using { get; set; }

    /// <summary>
    /// get/set - The draft name 'content.create' registers (e.g. '$item.digest').
    /// </summary>
    public string? As { get; set; }

    /// <summary>
    /// get/set - What 'content.create' copies from: '$item' (the subject) or empty for a blank item.
    /// </summary>
    public string? CopyFrom { get; set; }

    /// <summary>
    /// get/set - The fields 'content.create' copies from the CopyFrom source.
    /// </summary>
    public List<string>? CopyFields { get; set; }

    /// <summary>
    /// get/set - Field values 'content.create' sets, each from a fixed value source.
    /// </summary>
    public Dictionary<string, ValueSource>? Set { get; set; }

    /// <summary>
    /// get/set - Whether 'content.save' sends the indexing message (default true).
    /// </summary>
    public bool? Index { get; set; }

    /// <summary>
    /// get/set - Optional LLM override for this action's prompts (dedupe).
    /// </summary>
    public int? LlmId { get; set; }
    #endregion
}

/// <summary>
/// ValueSource class, where an action's value comes from. Exactly one of the three is set;
/// there is nothing to compute.
/// </summary>
public class ValueSource
{
    #region Properties
    /// <summary>
    /// get/set - 'analysisName.key' (an analysis result) or 'content.field' (the working copy).
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// get/set - A literal value (string, number, boolean, or array).
    /// </summary>
    public JsonElement? Literal { get; set; }

    /// <summary>
    /// get/set - A token template using the same substitution prompts use
    /// (e.g. 'DIGEST: {content.headline}').
    /// </summary>
    public string? Template { get; set; }
    #endregion
}
