namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIIndexSchema record, what the model is told about a content index's live mapping.
/// </summary>
/// <param name="Fields">Every queryable field, one per line (FilterAISchema.Read).</param>
/// <param name="CollapsibleFields">The fields a search can collapse on to return one story per value.</param>
public record FilterAIIndexSchema(string Fields, IReadOnlyList<string> CollapsibleFields)
{
    /// <summary>A mapping that could not be read.</summary>
    public static readonly FilterAIIndexSchema Empty = new("", Array.Empty<string>());
}
