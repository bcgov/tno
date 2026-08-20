namespace TNO.API.Areas.Admin.Models.Automation.V2;

/// <summary>
/// V2ActionDescriptor record, describes one action type: where it may appear, what it needs,
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
public record V2ActionDescriptor(
    string Type,
    string Label,
    string Category,
    bool RequiresSubject,
    bool RequiresPersistedId,
    bool UsesLLM,
    string[] Phases,
    V2FieldSpec[] Fields,
    string? Description = null);

/// <summary>
/// V2FieldSpec record, one configuration field of an action type.
/// Kind drives the editor control: 'filter', 'collection', 'string', 'int', 'bool', 'condition',
/// 'valueSource', 'valueMap', 'fields', 'truncateMap', 'contentField', 'report', 'notification',
/// 'contentAction', 'item', 'draft', or 'enum:a|b|c'.
/// </summary>
/// <param name="Name">The V2ActionDefinition property (camelCase, as serialized).</param>
/// <param name="Kind">The field kind.</param>
/// <param name="Required">Whether the field must be set.</param>
/// <param name="Help">Editor help text.</param>
public record V2FieldSpec(string Name, string Kind, bool Required, string? Help = null);

/// <summary>
/// V2ActionCatalog class, the registry of every v2 action type.
/// </summary>
public static class V2ActionCatalog
{
    private static readonly string[] _all = { V2Phases.Init, V2Phases.Process, V2Phases.Complete };
    // Per-item actions: process steps always iterate; complete steps iterate when they declare a source.
    private static readonly string[] _process = { V2Phases.Process, V2Phases.Complete };
    private static readonly string[] _once = { V2Phases.Init, V2Phases.Complete };

    /// <summary>
    /// Every registered action descriptor, keyed by type.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, V2ActionDescriptor> Types = new[]
    {
        new V2ActionDescriptor("search", "Search Content", "search", false, false, false, _all, new[]
        {
            new V2FieldSpec("filter", "filter", true, "The saved filter whose Elasticsearch query is executed."),
            new V2FieldSpec("into", "collection", true, "The collection the results are written to (e.g. $run.inbox)."),
            new V2FieldSpec("fields", "fields", false, "Digest fields to keep per item; defaults to the standard digest."),
            new V2FieldSpec("max", "int", false, "Maximum items to fetch (default 500)."),
            new V2FieldSpec("truncate", "truncateMap", false, "Per-field character caps applied on ingest."),
        },
            Description: "Runs the saved 'filter' query against the content index and writes a digest of each match into the 'into' collection. 'fields' limits which properties each digest keeps, 'max' caps how many items are fetched, and 'truncate' caps long text fields on ingest."),
        new V2ActionDescriptor("collection.create", "Create Collection", "collection", false, false, false, _all, new[]
        {
            new V2FieldSpec("into", "collection", true, "New collection name"),
        },
            Description: "Creates a new, empty named collection in the run context. 'into' names it; later actions reference that name to add, read, or combine items."),
        new V2ActionDescriptor("collection.add", "Add To Collection", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("into", "collection", true),
            new V2FieldSpec("item", "item", false, "What to add: the original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Adds the current item to the 'into' collection (duplicates are skipped). 'item' picks what is added: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new V2ActionDescriptor("collection.remove", "Remove From Collection", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("item", "item", false, "The original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Removes the current item from the 'from' collection. 'item' picks what is removed: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new V2ActionDescriptor("collection.move", "Move Between Collections", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("into", "collection", true),
            new V2FieldSpec("item", "item", false, "The original item (default), or a draft created by an earlier Create Content action in this step."),
        },
            Description: "Moves the current item out of the 'from' collection and into the 'into' collection. 'item' picks what moves: the original item (default) or a draft - a new content item produced by an earlier Create Content action in this step."),
        new V2ActionDescriptor("collection.filter", "Filter Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("where", "condition", true, "Items failing the condition are removed."),
        },
            Description: "Removes every item in the 'from' collection that fails the 'where' condition. Runs once against the whole collection."),
        new V2ActionDescriptor("collection.sortBy", "Sort Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("by", "string", true, "The digest field to sort by."),
            new V2FieldSpec("direction", "enum:asc|desc", false),
        },
            Description: "Reorders the 'from' collection by the digest field named in 'by'; 'direction' is ascending unless set to desc."),
        new V2ActionDescriptor("collection.take", "Take From Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("count", "int", true),
        },
            Description: "Keeps only the first 'count' items of the 'from' collection and drops the rest (usually after a sort)."),
        new V2ActionDescriptor("collection.distinctBy", "Distinct Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("by", "string", true),
        },
            Description: "Removes duplicates from the 'from' collection, keeping the first item for each distinct value of the 'by' field."),
        new V2ActionDescriptor("collection.union", "Union Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        },
            Description: "Writes every item of 'from' plus every item of 'with' into the 'into' collection, skipping duplicates."),
        new V2ActionDescriptor("collection.except", "Except Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        },
            Description: "Writes the items of 'from' that are NOT in 'with' into the 'into' collection - useful for new-items-only comparisons."),
        new V2ActionDescriptor("collection.intersect", "Intersect Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        },
            Description: "Writes the items present in BOTH 'from' and 'with' into the 'into' collection."),
        new V2ActionDescriptor("collection.save", "Save Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true, "The collection whose items are written to the database."),
        },
            Description: "Writes every changed item of the 'from' collection to the database - accumulated field changes update existing items and drafts are created. Changes not covered by a Save Collection or Save Content Now action are never written."),
        new V2ActionDescriptor("content.update", "Update Content Field", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("field", "contentField", true),
            new V2FieldSpec("value", "valueSource", true),
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets one property on the item's working copy. 'field' picks the property, 'value' supplies it (an analysis result, a literal, or a template), and 'target' redirects the write to a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item. Changes are written when the item is saved (a Save Collection or Save Content Now action)."),
        new V2ActionDescriptor("content.tags", "Add Tags", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "Tag codes (array or comma-separated)."),
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Adds tag codes to the working copy (existing tags are kept). 'value' supplies the codes from an analysis result, a literal list, or a template; 'target' applies them to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new V2ActionDescriptor("content.sentiment", "Set Sentiment", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "An integer from -5 to 5."),
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets the sentiment score on the working copy. 'value' supplies the number, usually from an analysis; 'target' applies it to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new V2ActionDescriptor("content.contributor", "Set Columnist/Contributor", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "A contributor name or alias."),
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Sets the columnist/contributor on the working copy. 'value' supplies a name or alias that is matched against the contributor list; 'target' applies it to a draft (a new content item produced by an earlier Create Content action in this step) instead."),
        new V2ActionDescriptor("content.action", "Apply Content Action", "content", true, true, false, _process, new[]
        {
            new V2FieldSpec("contentAction", "contentAction", true),
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Stamps a content action (editorial flag) on the item. 'contentAction' picks the flag; 'target' stamps a draft (a new content item produced by an earlier Create Content action in this step) instead. The item must already exist in the database."),
        new V2ActionDescriptor("content.publish", "Publish Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Marks the working copy as published; the status change is written when the item is saved (a Save Collection or Save Content Now action). 'target' publishes a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item."),
        new V2ActionDescriptor("content.unpublish", "Unpublish Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
        },
            Description: "Marks the working copy as unpublished; the status change is written when the item is saved (a Save Collection or Save Content Now action). 'target' unpublishes a draft (a new content item produced by an earlier Create Content action in this step) instead of the original item."),
        new V2ActionDescriptor("content.create", "Create Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("as", "draft", true, "The draft name later actions target (e.g. $item.digest)."),
            new V2FieldSpec("copyFrom", "item", false, "Copies from the original item; leave empty to start blank."),
            new V2FieldSpec("copyFields", "fields", false),
            new V2FieldSpec("set", "valueMap", false, "Field values from analysis results, literals, or templates."),
        },
            Description: "Creates a draft: a brand-new content item that later actions in this step target by the name given in 'as'. 'copyFrom' seeds the draft from the original item or leaves it blank; 'copyFields' limits which properties are copied; 'set' fills fields from analysis results, literals, or templates."),
        new V2ActionDescriptor("content.save", "Save Content Now", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false, "A draft created by an earlier Create Content action in this step; leave empty for the original item."),
            new V2FieldSpec("index", "bool", false, "Send the indexing message (default true)."),
        },
            Description: "Writes the working copy (or the draft named by 'target' - a new content item produced by an earlier Create Content action in this step) to the database immediately instead of waiting for a Save Collection action. 'index' controls whether the search-index message is sent (default true)."),
        new V2ActionDescriptor("exclude", "Exclude From Run", "flow", true, false, false, _process, new[]
        {
            new V2FieldSpec("reason", "string", false),
        },
            Description: "Removes the current item from the rest of the run - later steps skip it, but changes already made are kept and written. 'reason' is recorded in the run log."),
        new V2ActionDescriptor("abort", "Stop Remaining Actions", "flow", true, false, false, _process, System.Array.Empty<V2FieldSpec>(),
            Description: "Stops the remaining actions of this step for the current item; later steps still see the item."),
        new V2ActionDescriptor("dedupe", "Detect Duplicate", "flow", true, false, true, _process, new[]
        {
            new V2FieldSpec("against", "collection", true, "The collection of candidates to compare with."),
            new V2FieldSpec("mode", "enum:iterate|batch", false),
            new V2FieldSpec("batchSize", "int", false),
            new V2FieldSpec("maxComparisons", "int", false),
            new V2FieldSpec("onDuplicate", "enum:exclude|abort|remove", false),
            new V2FieldSpec("prompt", "prompt", false, "Comparison prompt override."),
        },
            Description: "Asks the LLM whether the current item duplicates any candidate in the 'against' collection. 'mode' compares one candidate per call (iterate, default) or 'batchSize' candidates per call (batch); 'maxComparisons' caps how many candidates are examined; 'onDuplicate' sets what happens on a match - exclude the item from the run (default), abort the step's remaining actions, or remove it from the step's source collection; 'prompt' overrides the comparison prompt."),
        new V2ActionDescriptor("score", "Score Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("objective", "string", true),
            new V2FieldSpec("value", "valueSource", true, "An integer score."),
        },
            Description: "Records an integer score for the current item under the named 'objective'. 'value' supplies the score, usually from an analysis; a later select-top action ranks by it."),
        new V2ActionDescriptor("select-top", "Select Top Scored", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("objective", "string", true),
            new V2FieldSpec("take", "int", true),
            new V2FieldSpec("into", "collection", false, "Collection the selected items are written to."),
            new V2FieldSpec("contentAction", "contentAction", false, "Content action stamped on each selected item."),
        },
            Description: "Ranks every item scored under 'objective' and keeps the best 'take'. 'into' writes the selected items to a collection; 'contentAction' stamps an editorial flag on each."),
        new V2ActionDescriptor("report.run", "Run Report", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("report", "report", true),
            new V2FieldSpec("using", "collection", false),
        },
            Description: "Queues the saved 'report' for generation and sending. 'using' records the collection that fed it in the run summary."),
        new V2ActionDescriptor("notification.run", "Run Notification", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("notification", "notification", true),
            new V2FieldSpec("using", "collection", false),
        },
            Description: "Queues the saved 'notification' for sending. 'using' records the collection that fed it in the run summary."),
    }.ToDictionary(d => d.Type, StringComparer.OrdinalIgnoreCase);
}
