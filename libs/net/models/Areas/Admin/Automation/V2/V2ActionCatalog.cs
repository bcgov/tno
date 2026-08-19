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
public record V2ActionDescriptor(
    string Type,
    string Label,
    string Category,
    bool RequiresSubject,
    bool RequiresPersistedId,
    bool UsesLLM,
    string[] Phases,
    V2FieldSpec[] Fields);

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
        }),
        new V2ActionDescriptor("collection.create", "Create Collection", "collection", false, false, false, _all, new[]
        {
            new V2FieldSpec("into", "collection", true, "The collection name to create (e.g. $run.published)."),
        }),
        new V2ActionDescriptor("collection.add", "Add To Collection", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("into", "collection", true),
            new V2FieldSpec("item", "item", false, "What to add: $item (default) or a draft name."),
        }),
        new V2ActionDescriptor("collection.remove", "Remove From Collection", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("item", "item", false),
        }),
        new V2ActionDescriptor("collection.move", "Move Between Collections", "collection", true, false, false, _process, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("into", "collection", true),
            new V2FieldSpec("item", "item", false),
        }),
        new V2ActionDescriptor("collection.filter", "Filter Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("where", "condition", true, "Items failing the condition are removed."),
        }),
        new V2ActionDescriptor("collection.sortBy", "Sort Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("by", "string", true, "The digest field to sort by."),
            new V2FieldSpec("direction", "enum:asc|desc", false),
        }),
        new V2ActionDescriptor("collection.take", "Take From Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("count", "int", true),
        }),
        new V2ActionDescriptor("collection.distinctBy", "Distinct Collection", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("by", "string", true),
        }),
        new V2ActionDescriptor("collection.union", "Union Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        }),
        new V2ActionDescriptor("collection.except", "Except Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        }),
        new V2ActionDescriptor("collection.intersect", "Intersect Collections", "collection", false, false, false, _once, new[]
        {
            new V2FieldSpec("from", "collection", true),
            new V2FieldSpec("with", "collection", true),
            new V2FieldSpec("into", "collection", true),
        }),
        new V2ActionDescriptor("content.update", "Update Content Field", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("field", "contentField", true),
            new V2FieldSpec("value", "valueSource", true),
            new V2FieldSpec("target", "draft", false, "A draft name; omitted for the subject."),
        }),
        new V2ActionDescriptor("content.tags", "Add Tags", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "Tag codes (array or comma-separated)."),
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.sentiment", "Set Sentiment", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "An integer from -5 to 5."),
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.contributor", "Set Columnist/Contributor", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("value", "valueSource", true, "A contributor name or alias."),
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.action", "Apply Content Action", "content", true, true, false, _process, new[]
        {
            new V2FieldSpec("contentAction", "contentAction", true),
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.publish", "Publish Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.unpublish", "Unpublish Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false),
        }),
        new V2ActionDescriptor("content.create", "Create Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("as", "draft", true, "The draft name later actions target (e.g. $item.digest)."),
            new V2FieldSpec("copyFrom", "item", false, "$item to copy from the subject; empty for a blank item."),
            new V2FieldSpec("copyFields", "fields", false),
            new V2FieldSpec("set", "valueMap", false, "Field values from analysis results, literals, or templates."),
        }),
        new V2ActionDescriptor("content.save", "Save Content Now", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("target", "draft", false),
            new V2FieldSpec("index", "bool", false, "Send the indexing message (default true)."),
        }),
        new V2ActionDescriptor("exclude", "Exclude From Run", "flow", true, false, false, _process, new[]
        {
            new V2FieldSpec("reason", "string", false),
        }),
        new V2ActionDescriptor("abort", "Stop Remaining Actions", "flow", true, false, false, _process, System.Array.Empty<V2FieldSpec>()),
        new V2ActionDescriptor("dedupe", "Detect Duplicate", "flow", true, false, true, _process, new[]
        {
            new V2FieldSpec("against", "collection", true, "The collection of candidates to compare with."),
            new V2FieldSpec("mode", "enum:iterate|batch", false),
            new V2FieldSpec("batchSize", "int", false),
            new V2FieldSpec("maxComparisons", "int", false),
            new V2FieldSpec("onDuplicate", "enum:exclude|abort|remove", false),
            new V2FieldSpec("prompt", "prompt", false, "Comparison prompt override."),
        }),
        new V2ActionDescriptor("score", "Score Content", "content", true, false, false, _process, new[]
        {
            new V2FieldSpec("objective", "string", true),
            new V2FieldSpec("value", "valueSource", true, "An integer score."),
        }),
        new V2ActionDescriptor("select-top", "Select Top Scored", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("objective", "string", true),
            new V2FieldSpec("take", "int", true),
            new V2FieldSpec("into", "collection", false, "Collection the selected items are written to."),
            new V2FieldSpec("contentAction", "contentAction", false, "Content action stamped on each selected item."),
        }),
        new V2ActionDescriptor("report.run", "Run Report", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("report", "report", true),
            new V2FieldSpec("using", "collection", false),
        }),
        new V2ActionDescriptor("notification.run", "Run Notification", "distribute", false, true, false, _once, new[]
        {
            new V2FieldSpec("notification", "notification", true),
            new V2FieldSpec("using", "collection", false),
        }),
    }.ToDictionary(d => d.Type, StringComparer.OrdinalIgnoreCase);
}
