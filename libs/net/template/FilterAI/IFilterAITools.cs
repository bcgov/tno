using System.Text.Json.Nodes;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// IFilterAITools interface, the read-only tools a model may use while it builds a filter. The
/// caller decides which index and which restrictions apply (an editor and a subscriber see
/// different content).
/// </summary>
public interface IFilterAITools
{
    /// <summary>
    /// Find reference values of the specified kind whose name or code contains the text.
    /// </summary>
    /// <param name="kind">One of FilterAIReferenceKind.</param>
    /// <param name="text">The text to match; empty lists the first values.</param>
    /// <param name="limit">The most values to return.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<FilterAIReference>> LookupAsync(string kind, string? text, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Read the fields of the content index searches run against, from its live mapping
    /// (FilterAISchema.Read).
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FilterAIIndexSchema> GetIndexSchemaAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Run a search body built by FilterAISearch.BuildSearchBody and return the raw response JSON.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> SearchAsync(JsonObject body, CancellationToken cancellationToken);

    /// <summary>
    /// Whether searches include unpublished content (drafts), as the filter will.
    /// </summary>
    bool SearchesUnpublished { get; }

    /// <summary>
    /// Count the stories a search body matches among all content, drafts included, when searches
    /// only cover published content; otherwise null.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long?> CountUnpublishedAsync(JsonObject body, CancellationToken cancellationToken);

    /// <summary>
    /// Validate a query body ({ "query": ... }); returns null when it is valid, otherwise why not.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> ValidateAsync(JsonObject body, CancellationToken cancellationToken);

    /// <summary>
    /// Describe every reference in the settings that does not exist (i.e. "source id 12").
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<string>> FindUnknownReferencesAsync(FilterSettingsModel settings, CancellationToken cancellationToken);
}
