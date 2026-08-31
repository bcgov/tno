namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ActionDescriptor record, describes one action type: where it may appear, what it needs,
/// and which configuration fields it reads. The catalog is the single source of truth - the
/// validator checks definitions against it, the engine dispatches by it, and the editor renders
/// action forms from it (served by the descriptors endpoint).
/// </summary>
/// <param name="Type">The action type key.</param>
/// <param name="Label">Display label.</param>
/// <param name="Category">Grouping for the editor ('content', 'collection', 'flow', 'search', 'distribute').</param>
/// <param name="RequiresSubject">Whether the action needs an iterated item (process phase only).</param>
/// <param name="RequiresPersistedId">Whether the action needs its target to have a database id.</param>
/// <param name="UsesLLM">Whether the action itself sends prompts (dedupe).</param>
/// <param name="Phases">The phases the action may appear in.</param>
/// <param name="Fields">The configuration fields the action reads.</param>
/// <param name="Description">How the action works and what each field does (shown in the editor).</param>
public record ActionDescriptor(
    string Type,
    string Label,
    string Category,
    bool RequiresSubject,
    bool RequiresPersistedId,
    bool UsesLLM,
    string[] Phases,
    FieldSpec[] Fields,
    string? Description = null);

/// <summary>
/// FieldSpec record, one configuration field of an action type.
/// Kind drives the editor control: 'filter', 'collection', 'string', 'int', 'bool', 'condition',
/// 'valueSource', 'valueMap', 'fields', 'truncateMap', 'contentField', 'report', 'notification',
/// 'contentAction', 'contentActionValue', 'item', 'draft', or 'enum:a|b|c'.
/// 'contentActionValue' is a value source whose control follows the picked content action's own
/// value type: a true/false toggle for a boolean action, the full value source for the rest.
/// </summary>
/// <param name="Name">The ActionDefinition property (camelCase, as serialized).</param>
/// <param name="Kind">The field kind.</param>
/// <param name="Required">Whether the field must be set.</param>
/// <param name="Help">Editor help text.</param>
public record FieldSpec(string Name, string Kind, bool Required, string? Help = null);

/// <summary>
/// ActionCatalog class, the registry of every action type.
/// </summary>
public static class ActionCatalog
{
    private static readonly string[] _all = { AutomationPhases.Init, AutomationPhases.Process, AutomationPhases.Complete };
    // Per-item actions: process steps always iterate; complete steps iterate when they declare a source.
    private static readonly string[] _process = { AutomationPhases.Process, AutomationPhases.Complete };
    private static readonly string[] _once = { AutomationPhases.Init, AutomationPhases.Complete };

    /// <summary>
    /// Every registered action descriptor, keyed by type.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, ActionDescriptor> Types = new[]
    {
        new ActionDescriptor("search", "Search Content", "search", false, false, false, _all, new[]
        {
            new FieldSpec("filter", "filter", true, "The saved filter whose Elasticsearch query is executed."),
            new FieldSpec("into", "collection", true, "The collection the results are written to (e.g. $run.inbox)."),
            new FieldSpec("fields", "fields", false, "Digest fields to keep per item; defaults to the standard digest."),
            new FieldSpec("max", "int", false, "Maximum items to fetch (default 500)."),
            new FieldSpec("truncate", "truncateMap", false, "Per-field character caps applied on ingest."),
        },
            Description: "Runs the saved 'filter' query against the content index and writes a digest of each match into the 'into' collection. 'fields' limits which properties each digest keeps, 'max' caps how many items are fetched, and 'truncate' caps long text fields on ingest."),
        new ActionDescriptor("collection.create", "Create Collection", "collection", false, false, false, _all, new[]
        {
            new FieldSpec("into", "collection", true, "New collection name"),
        },
            Description: "Creates a new, empty named collection in the run context. 'into' names it; later actions reference that name to add, read, or combine items."),
        new ActionDescriptor("collection.add", "Add To Collection", "collection", true, false, false, _process, new[]
        {
            new FieldSpec("into", "collection", true),
            new FieldSpec("item", "item", false, "What to add: the original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Adds the current item to the 'into' collection (duplicates are skipped). 'item' picks what is added: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new ActionDescriptor("collection.remove", "Remove From Collection", "collection", true, false, false, _process, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("item", "item", false, "The original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Removes the current item from the 'from' collection. 'item' picks what is removed: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new ActionDescriptor("collection.move", "Move Between Collections", "collection", true, false, false, _process, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("into", "collection", true),
            new FieldSpec("item", "item", false, "The original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Moves the current item out of the 'from' collection and into the 'into' collection. 'item' picks what moves: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new ActionDescriptor("collection.copy", "Copy Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true, "The collection whose items are copied; it is unchanged."),
            new FieldSpec("into", "collection", true, "The destination collection - an existing one or a new name."),
        },
            Description: "Copies every item of the 'from' collection into the 'into' collection - an existing collection or a new name typed into the field. Items already in the destination are skipped, and the source is unchanged."),
        new ActionDescriptor("collection.filter", "Filter Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("where", "condition", true, "Items failing the condition are removed."),
        },
            Description: "Removes every item in the 'from' collection that fails the 'where' condition. Runs once against the whole collection."),
        new ActionDescriptor("collection.sortBy", "Sort Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("by", "string", true, "The digest field to sort by."),
            new FieldSpec("direction", "enum:asc|desc", false),
        },
            Description: "Reorders the 'from' collection by the digest field named in 'by'; 'direction' is ascending unless set to desc."),
        new ActionDescriptor("collection.take", "Take From Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("count", "int", true),
        },
            Description: "Keeps only the first 'count' items of the 'from' collection and drops the rest (usually after a sort)."),
        new ActionDescriptor("collection.distinctBy", "Distinct Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("by", "string", true),
        },
            Description: "Removes duplicates from the 'from' collection, keeping the first item for each distinct value of the 'by' field."),
        new ActionDescriptor("collection.union", "Union Collections", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("with", "collection", true),
            new FieldSpec("into", "collection", true),
        },
            Description: "Writes every item of 'from' plus every item of 'with' into the 'into' collection, skipping duplicates."),
        new ActionDescriptor("collection.except", "Except Collections", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("with", "collection", true),
            new FieldSpec("into", "collection", true),
        },
            Description: "Writes the items of 'from' that are NOT in 'with' into the 'into' collection - useful for new-items-only comparisons."),
        new ActionDescriptor("collection.intersect", "Intersect Collections", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true),
            new FieldSpec("with", "collection", true),
            new FieldSpec("into", "collection", true),
        },
            Description: "Writes the items present in BOTH 'from' and 'with' into the 'into' collection."),
        new ActionDescriptor("collection.save", "Save Collection", "collection", false, false, false, _once, new[]
        {
            new FieldSpec("from", "collection", true, "The collection whose items are written to the database."),
            new FieldSpec("index", "bool", false, "Send the indexing message so Elasticsearch gets the saved content (default true)."),
        },
            Description: "Writes every changed item of the 'from' collection to the database - accumulated field changes update existing items and drafts are created. 'index' sends the search-index message for each saved item (default true) so Elasticsearch receives the changes. Changes not covered by a Save Collection or Save Content Now action are never written."),
        new ActionDescriptor("content.update", "Update Content Field", "content", true, false, false, _process, new[]
        {
            new FieldSpec("field", "contentField", true),
            new FieldSpec("value", "valueSource", true),
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets one property on the item's working copy. 'field' picks the property, 'value' supplies it (an analysis result, a literal, or a template), and 'target' redirects the write to a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item. Changes are written when the item is saved (a Save Collection or Save Content Now action)."),
        new ActionDescriptor("content.tags", "Add Tags", "content", true, false, false, _process, new[]
        {
            new FieldSpec("value", "valueSource", true, "Tag codes (array or comma-separated)."),
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Adds tag codes to the working copy (existing tags are kept). 'value' supplies the codes from an analysis result, a literal list, or a template; 'target' applies them to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new ActionDescriptor("content.sentiment", "Set Sentiment", "content", true, false, false, _process, new[]
        {
            new FieldSpec("value", "valueSource", true, "An integer from -5 to 5."),
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets the sentiment score on the working copy. 'value' supplies the number, usually from an analysis; 'target' applies it to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new ActionDescriptor("content.contributor", "Set Columnist/Contributor", "content", true, false, false, _process, new[]
        {
            new FieldSpec("value", "valueSource", true, "A contributor name or alias."),
            new FieldSpec("create", "bool", false, "Create the contributor when no enabled record matches (real runs create immediately; dry runs log the intent). Off: unmatched values are skipped."),
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets the columnist/contributor on the working copy. 'value' supplies a name or alias that is matched against the enabled contributor list; 'create' adds a new contributor record when nothing matches (otherwise unmatched values are skipped); 'target' applies it to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new ActionDescriptor("content.action", "Apply Content Action", "content", true, true, false, _process, new[]
        {
            new FieldSpec("contentAction", "contentAction", true),
            new FieldSpec("value", "contentActionValue", false, "What the action stores. A yes/no action stores true or false; an action that records a value (Commentary's timeout in days, for example) needs that value."),
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Stamps a content action (editorial flag) on the item. 'contentAction' picks the flag and 'value' supplies what it stores: a yes/no action stores true or false, while an action that records a value (Commentary's timeout in days, for example) takes it from an analysis result, a literal, or a template - such an action stamps nothing when no value resolves. 'target' stamps a draft (a new content item produced by an earlier Create Content action in this step) instead. The item must already exist in the database."),
        new ActionDescriptor("content.publish", "Publish Content", "content", true, false, false, _process, new[]
        {
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Marks the working copy as published; the status change is written when the item is saved (a Save Collection or Save Content Now action). 'target' publishes a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item."),
        new ActionDescriptor("content.unpublish", "Unpublish Content", "content", true, false, false, _process, new[]
        {
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Marks the working copy as unpublished; the status change is written when the item is saved (a Save Collection or Save Content Now action). 'target' unpublishes a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item."),
        new ActionDescriptor("content.create", "Create Content", "content", true, false, false, _process, new[]
        {
            new FieldSpec("as", "draft", true, "The draft name later actions target (e.g. $item.digest)."),
            new FieldSpec("copyFrom", "item", false, "Copies from the original item; leave empty to start blank."),
            new FieldSpec("copyFields", "fields", false, "Leave empty to copy the defaults: sourceId, otherSource, licenseId, mediaTypeId, publishedOn, contentType. Check 'all fields' (stored as *) to copy everything the item carries. A derived uid is always set."),
            new FieldSpec("set", "valueMap", false, "Field values from analysis results, literals, or templates."),
        },
            Description: "Creates a draft: a brand-new content item that later actions in this step target by the name given in 'as'. 'copyFrom' seeds the draft from the original item or leaves it blank; 'copyFields' limits which properties are copied; 'set' fills fields from analysis results, literals, or templates."),
        new ActionDescriptor("content.save", "Save Content Now", "content", true, false, false, _process, new[]
        {
            new FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
            new FieldSpec("index", "bool", false, "Send the indexing message (default true)."),
        },
            Description: "Writes the working copy (or the draft named by 'target' - a new content item produced by an earlier Create Content action in this step) to the database immediately instead of waiting for a Save Collection action. 'index' controls whether the search-index message is sent (default true)."),
        new ActionDescriptor("exclude", "Exclude From Run", "flow", true, false, false, _process, new[]
        {
            new FieldSpec("reason", "string", false),
        },
            Description: "Removes the current item from the rest of the run - later steps skip it, but changes already made are kept and written. 'reason' is recorded in the run log."),
        new ActionDescriptor("abort", "Stop Remaining Actions", "flow", true, false, false, _process, System.Array.Empty<FieldSpec>(),
            Description: "Stops the remaining actions of this step for the current item; later steps still see the item."),
        new ActionDescriptor("dedupe", "Detect Duplicate", "flow", true, false, true, _process, new[]
        {
            new FieldSpec("against", "collection", true, "The collection of candidates to compare with."),
            new FieldSpec("mode", "enum:iterate|batch", false),
            new FieldSpec("batchSize", "int", false),
            new FieldSpec("maxComparisons", "int", false),
            new FieldSpec("remember", "bool", false, "Persist confirmed duplicates as content_link records (value 'duplicate') and skip the LLM for items already linked. Links are written on real runs only; dry runs read them."),
            new FieldSpec("prompt", "prompt", false, "Comparison prompt; empty uses the 'default-dedupe' library entry when present, else the built-in comparison. {content.*} and {candidate.*} tokens give a custom layout; without tokens both stories are appended automatically."),
        },
            Description: "Asks the LLM whether the current item duplicates any candidate in the 'against' collection, and records the answer for later actions to route on: '<action name>.isDuplicate' (true/false) and '<action name>.matchedId' (the matched candidate's id). The action decides nothing itself - gate later actions with Runs when = Condition and an Analysis answer of '<action name>.isDuplicate' (e.g. add duplicates to a collection or Exclude From Run; use Not for the unique items). 'mode' compares one candidate per call (iterate, default) or 'batchSize' candidates per call (batch); 'maxComparisons' caps how many candidates are examined; 'remember' persists confirmed duplicates as content_link records so later runs skip the LLM for already-linked items; 'prompt' overrides the comparison prompt."),
        new ActionDescriptor("score", "Score Content", "content", true, false, false, _process, new[]
        {
            new FieldSpec("objective", "string", true, "Names the score, e.g. 'top-story'. Use the same name on the Select Top Scored action that consumes it."),
            new FieldSpec("value", "valueSource", true, "Where the integer score comes from - normally an analysis result, '<analysis name>.<key>'."),
        },
            Description: "Records an integer score for the current item under the named 'objective' - the number a later Select Top Scored action ranks by. This action does not call the LLM itself; it stores whatever 'value' resolves to. TO SCORE STORIES WITH A PROMPT: (1) add an Analysis to this step whose prompt asks for the score and whose Returns declares the key and range, e.g. key 'score' of type 'int(1..10)'; (2) set this action's Value to 'Analysis result / content field' and pick '<analysis name>.score'; (3) name the Objective and use that same name on a Select Top Scored action in a later (complete) step. The analysis runs once per item, only when an action consumes it. An answer that is not a whole number is logged as skipped and the item stays unscored, so the Returns range is what keeps the model in bounds. Rescoring an item replaces its earlier score, and the run outcome lists every scored story with its score plus how many stories carried each score."),
        new ActionDescriptor("select-top", "Select Top Scored", "distribute", false, true, false, _once, new[]
        {
            new FieldSpec("objective", "string", true, "The objective whose scores are ranked - the same name a Score Content action recorded."),
            new FieldSpec("take", "int", false, "Keep this many items. Required unless 'minScore' is set; set both to cap how many qualifying items are kept."),
            new FieldSpec("minScore", "int", false, "Keep every item scoring at or above this value, however many that is. Leave empty to select a fixed count with 'take' instead."),
            new FieldSpec("into", "collection", false, "Collection the selected items are written to."),
            new FieldSpec("contentAction", "contentAction", false, "Content action stamped on each selected item."),
            new FieldSpec("value", "contentActionValue", false, "What the stamped content action stores. A yes/no action stores true or false; an action that records a value (Commentary's timeout in days, for example) needs that value."),
        },
            Description: "Selects items from the scores a Score Content action recorded under 'objective'. No LLM is involved - it ranks the recorded scores highest first and breaks ties on the lowest content id, so the same scores always select the same items. Choose how many to keep: 'take' keeps a fixed count (the top 10); 'minScore' keeps every item scoring at or above a threshold, so a day with more good stories yields more selections; setting both keeps the qualifying items up to the 'take' cap. 'into' writes the selected items to a collection; 'contentAction' stamps an editorial flag on each and 'value' supplies what that flag stores (true/false for a yes/no action, otherwise the value it records - a literal is the usual choice here, since the selection runs after the per-item analyses). The run outcome and decision log name the selected items and report how many items carried each score."),
        new ActionDescriptor("report.run", "Run Report", "distribute", false, true, false, _once, new[]
        {
            new FieldSpec("report", "report", true),
            new FieldSpec("using", "collection", false),
        },
            Description: "Queues the saved 'report' for generation and sending. 'using' records the collection that fed it in the run summary."),
        new ActionDescriptor("notification.run", "Run Notification", "distribute", false, true, false, _once, new[]
        {
            new FieldSpec("notification", "notification", true),
            new FieldSpec("using", "collection", false),
        },
            Description: "Queues the saved 'notification' for sending. 'using' records the collection that fed it in the run summary."),
    }.ToDictionary(d => d.Type, StringComparer.OrdinalIgnoreCase);
}
