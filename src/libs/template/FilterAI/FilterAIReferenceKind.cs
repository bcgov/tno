namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIReferenceKind class, the kinds of reference value the lookup tool finds.
/// </summary>
public static class FilterAIReferenceKind
{
    /// <summary>Sources (settings.sourceIds).</summary>
    public const string Source = "source";
    /// <summary>Media types (settings.mediaTypeIds).</summary>
    public const string MediaType = "mediaType";
    /// <summary>Series and programs (settings.seriesIds).</summary>
    public const string Series = "series";
    /// <summary>Contributors and columnists (settings.contributorIds).</summary>
    public const string Contributor = "contributor";
    /// <summary>Tags (settings.tags, by code).</summary>
    public const string Tag = "tag";
    /// <summary>Actions (settings.actions).</summary>
    public const string Action = "action";

    /// <summary>Every kind.</summary>
    public static readonly string[] All = [Source, MediaType, Series, Contributor, Tag, Action];
}
