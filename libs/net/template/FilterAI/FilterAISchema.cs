using System.Text;
using System.Text.Json;

namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAISchema class, describes a content index's live mapping for a model: every field it can
/// query, its type, its exact-match subfields, and which objects are nested (and so need a nested
/// query). Bookkeeping fields that no filter would use are left out to keep the description short.
/// </summary>
public static class FilterAISchema
{
    #region Variables
    /// <summary>Field names left out wherever they appear inside an object (audit and join columns).</summary>
    public static readonly string[] OmittedChildFields = ["createdBy", "createdById", "createdOn", "updatedBy", "updatedById", "updatedOn", "version", "contentId", "sortOrder", "isEnabled"];

    /// <summary>The field types a search can collapse on (one story per value).</summary>
    public static readonly string[] CollapsibleTypes = ["keyword", "long", "integer", "short", "byte", "date", "boolean"];

    /// <summary>Whole objects left out (configuration and settings blobs).</summary>
    public static readonly string[] OmittedObjects = ["source.configuration", "mediaType.settings", "versions", "owner.preferences", "fileReferences", "timeTrackings", "notifications", "userNotifications"];
    #endregion

    #region Methods
    /// <summary>
    /// Describe the fields of the first index in a '_mapping' response, one per line:
    /// "path: type", with "(nested)" on nested objects and the exact-match subfields.
    /// </summary>
    /// <param name="mappingResponse"></param>
    /// <returns></returns>
    public static string DescribeMapping(string mappingResponse) => Read(mappingResponse).Fields;

    /// <summary>
    /// Read the first index in a '_mapping' response: the description of its fields, and the
    /// fields a search can collapse on (single-valued keyword, number, date and boolean fields
    /// outside nested objects, including exact-match subfields of text fields).
    /// </summary>
    /// <param name="mappingResponse"></param>
    /// <returns></returns>
    public static FilterAIIndexSchema Read(string mappingResponse)
    {
        using var document = JsonDocument.Parse(mappingResponse);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) return FilterAIIndexSchema.Empty;
        var index = root.EnumerateObject().FirstOrDefault();
        if (index.Value.ValueKind != JsonValueKind.Object
            || !index.Value.TryGetProperty("mappings", out var mappings)
            || !mappings.TryGetProperty("properties", out var properties))
            return FilterAIIndexSchema.Empty;

        var sb = new StringBuilder();
        var collapsible = new List<string>();
        Describe(sb, collapsible, properties, "", inNested: false);
        return new FilterAIIndexSchema(sb.ToString(), collapsible);
    }

    private static void Describe(StringBuilder sb, List<string> collapsible, JsonElement properties, string prefix, bool inNested)
    {
        foreach (var property in properties.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var path = $"{prefix}{property.Name}";
            if (OmittedObjects.Contains(path)) continue;
            if (prefix.Length > 0 && OmittedChildFields.Contains(property.Name)) continue;

            var field = property.Value;
            var type = field.TryGetProperty("type", out var t) ? t.GetString() ?? "object" : "object";
            if (field.TryGetProperty("properties", out var children))
            {
                sb.AppendLine(type == "nested" ? $"{path}: nested (query with {{ \"nested\": {{ \"path\": \"{path}\", ... }} }})" : $"{path}: object");
                Describe(sb, collapsible, children, $"{path}.", inNested || type == "nested");
                continue;
            }
            if (type == "object") continue; // An object with no mapped fields cannot be queried.

            var subfields = field.TryGetProperty("fields", out var f) && f.ValueKind == JsonValueKind.Object
                ? f.EnumerateObject().Select(s => $"{path}.{s.Name}").ToArray()
                : Array.Empty<string>();
            if (!inNested)
            {
                if (CollapsibleTypes.Contains(type)) collapsible.Add(path);
                else if (f.ValueKind == JsonValueKind.Object)
                    collapsible.AddRange(f.EnumerateObject()
                        .Where(s => s.Value.TryGetProperty("type", out var st) && CollapsibleTypes.Contains(st.GetString()))
                        .Select(s => $"{path}.{s.Name}"));
            }
            sb.AppendLine(subfields.Length > 0 ? $"{path}: {type} (exact match: {String.Join(", ", subfields)})" : $"{path}: {type}");
        }
    }
    #endregion
}
