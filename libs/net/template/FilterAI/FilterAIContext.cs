namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIContext record, what the model is told about the application for one conversation.
/// </summary>
/// <param name="Today">Today's date in the application's time zone.</param>
/// <param name="Schema">The content index's fields from its live mapping (FilterAISchema.Read).</param>
/// <param name="ReferenceValues">The actions, media types, content types and tags, with their ids and codes.</param>
/// <param name="SupportsBodyLength">Whether the filter page applies minBodyLength and minBodyWords (the editor does; the subscriber does not).</param>
/// <param name="SearchesUnpublished">Whether the filter (and the search tool) include unpublished content (drafts).</param>
public record FilterAIContext(DateTime Today, FilterAIIndexSchema Schema, string ReferenceValues, bool SupportsBodyLength, bool SearchesUnpublished);
