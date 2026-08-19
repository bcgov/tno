using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TNO.API.Areas.Admin.Models.Automation.V2;
using LookupModel = TNO.API.Areas.Editor.Models.Lookup.LookupModel;

namespace TNO.Services.Automation.V2;

/// <summary>
/// V2PromptBuilder class, resolves prompt text (library reference + override or inline text) and
/// substitutes runtime tokens:
/// - {content} and {content.field} - the subject's working copy (deltas folded in);
/// - {lookup:name} / {lookup:name[col,col]} - reference data from the run's lookup bundle,
///   enabled records only, stable order, size-guarded (so prompts stop pasting tag lists);
/// - {collection:$run.name} / {collection:$run.name[field,field]} - a collection's digests.
/// Lookup blocks belong in shared prompt text: they are identical for every item, so providers
/// can prefix-cache them across hundreds of calls.
/// </summary>
public class V2PromptBuilder
{
    /// <summary>Rows rendered per lookup list before truncation (marked, never silent).</summary>
    private const int MaxLookupRows = 500;

    /// <summary>Items rendered per collection injection before truncation.</summary>
    private const int MaxCollectionItems = 200;

    private static readonly Regex _lookupToken = new(@"\{lookup:(?<name>[a-zA-Z]+)(\[(?<cols>[^\]]+)\])?\}", RegexOptions.Compiled);
    private static readonly Regex _collectionToken = new(@"\{collection:(?<name>\$run\.[a-zA-Z0-9_-]+)(\[(?<cols>[^\]]+)\])?\}", RegexOptions.Compiled);
    private static readonly Regex _contentFieldToken = new(@"\{content\.(?<field>[a-zA-Z.]+)\}", RegexOptions.Compiled);

    private readonly AutomationDefinition _definition;
    private readonly LookupModel? _lookups;
    private readonly V2RunContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public V2PromptBuilder(AutomationDefinition definition, LookupModel? lookups, V2RunContext context, JsonSerializerOptions jsonOptions)
    {
        _definition = definition;
        _lookups = lookups;
        _context = context;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Resolve a prompt definition to text: library entry + layered override, or inline text.
    /// HTML (from rich-text editors) is converted to plain text.
    /// </summary>
    public string Resolve(V2PromptDefinition prompt)
    {
        var text = "";
        if (!string.IsNullOrWhiteSpace(prompt.Ref) && _definition.Prompts.TryGetValue(prompt.Ref!, out var library))
            text = library.Text;
        if (!string.IsNullOrWhiteSpace(prompt.Text))
            text = string.IsNullOrEmpty(text) ? prompt.Text! : $"{text}\n\n{prompt.Text}";
        if (!string.IsNullOrWhiteSpace(prompt.Override))
            text = string.IsNullOrEmpty(text) ? prompt.Override! : $"{text}\n\n{prompt.Override}";
        return PromptComposer.HtmlToText(text);
    }

    /// <summary>
    /// Substitute runtime tokens into resolved prompt text for the specified subject.
    /// When the text carries no {content} token and a subject exists, the working copy is
    /// appended as a '## News Story' section so the model always sees the item.
    /// </summary>
    public string Substitute(string text, V2ContentEntry? subject)
    {
        var result = _lookupToken.Replace(text, match => RenderLookup(match.Groups["name"].Value, SplitColumns(match)));
        result = _collectionToken.Replace(result, match => RenderCollection(match.Groups["name"].Value, SplitColumns(match)));

        if (subject != null)
        {
            result = _contentFieldToken.Replace(result, match => subject.GetField(match.Groups["field"].Value) ?? "");
            var json = subject.ToWorkingJson(_jsonOptions);
            result = result.Contains("{content}")
                ? result.Replace("{content}", json)
                : $"{result}\n\n## News Story\n{json}";
        }
        return result;
    }

    private static string[]? SplitColumns(Match match)
    {
        if (!match.Groups["cols"].Success) return null;
        return match.Groups["cols"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Render a lookup list: enabled records only, sorted for a stable (cacheable) prompt prefix.
    /// The same lookup bundle validates responses, so prompt and validation cannot disagree.
    /// </summary>
    private string RenderLookup(string name, string[]? columns)
    {
        if (_lookups == null) return $"(lookup '{name}' unavailable)";
        IEnumerable<(string Code, string Name, string Description)> rows = name.ToLowerInvariant() switch
        {
            "tags" => _lookups.Tags.Where(t => t.IsEnabled).OrderBy(t => t.Code, StringComparer.OrdinalIgnoreCase).Select(t => (t.Code, t.Name, t.Description)),
            "contributors" => _lookups.Contributors.Where(c => c.IsEnabled).OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Select(c => (c.Name, c.Name, c.Description)),
            "sources" => _lookups.Sources.Where(s => s.IsEnabled).OrderBy(s => s.Code, StringComparer.OrdinalIgnoreCase).Select(s => (s.Code, s.Name, s.Description)),
            "mediatypes" => _lookups.MediaTypes.Where(m => m.IsEnabled).OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).Select(m => (m.Name, m.Name, m.Description)),
            "actions" => _lookups.Actions.Where(a => a.IsEnabled).OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).Select(a => (a.Name, a.Name, a.Description)),
            "topics" => _lookups.Topics.Where(t => t.IsEnabled).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).Select(t => (t.Name, t.Name, t.Description)),
            _ => Enumerable.Empty<(string, string, string)>(),
        };

        var list = rows.ToList();
        if (list.Count == 0) return $"(lookup '{name}' has no enabled records)";

        var sb = new StringBuilder();
        foreach (var row in list.Take(MaxLookupRows))
        {
            if (columns == null)
            {
                sb.Append(row.Code);
                if (!string.Equals(row.Code, row.Name, StringComparison.Ordinal)) sb.Append(" | ").Append(row.Name);
                if (!string.IsNullOrWhiteSpace(row.Description)) sb.Append(" — ").Append(row.Description);
            }
            else
            {
                var parts = columns.Select(c => c.ToLowerInvariant() switch
                {
                    "code" => row.Code,
                    "name" => row.Name,
                    "description" => row.Description,
                    _ => "",
                });
                sb.Append(string.Join(" | ", parts));
            }
            sb.AppendLine();
        }
        if (list.Count > MaxLookupRows) sb.AppendLine($"…[{list.Count - MaxLookupRows} more entries truncated]");
        return sb.ToString().TrimEnd();
    }

    private string RenderCollection(string name, string[]? fields)
    {
        List<V2ContentEntry> snapshot;
        lock (_context.Sync)
        {
            snapshot = _context.Collections.TryGetValue(name, out var list) ? list.ToList() : new List<V2ContentEntry>();
        }
        var items = snapshot.Take(MaxCollectionItems).Select(entry =>
        {
            var digest = fields == null
                ? new Dictionary<string, string?>(entry.Digest, StringComparer.OrdinalIgnoreCase)
                : fields.ToDictionary(f => f, f => entry.GetField(f), StringComparer.OrdinalIgnoreCase);
            digest["id"] = entry.Kind == "draft" ? entry.TempKey : entry.Id.ToString();
            return digest;
        }).ToList();
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        return snapshot.Count > MaxCollectionItems
            ? $"{json}\n…[{snapshot.Count - MaxCollectionItems} more items truncated]"
            : json;
    }
}
