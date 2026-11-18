using System.Text.Json;
using System.Text.Json.Nodes;

namespace TNO.DAL.Services;

/// <summary>
/// LLMUsageJson class, reads and changes the LLM connections stored in JSON settings.
/// The JSON is edited in place so that every other value is kept exactly as it was.
/// </summary>
internal static class LLMUsageJson
{
    private const string LLMIdKey = "llmId";
    private const string TemperatureKey = "temperature";

    /// <summary>
    /// Describe where an automation definition overrides its connection with the specified one.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="llmId"></param>
    /// <returns></returns>
    public static IEnumerable<string> FindDefinitionReferences(JsonDocument? definition, int llmId)
    {
        var root = ToNode(definition);
        return root == null
            ? Array.Empty<string>()
            : DefinitionHolders(root).Where(h => GetId(h.Holder) == llmId).Select(h => h.Description).ToArray();
    }

    /// <summary>
    /// Move the overrides in an automation definition to another connection.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="llmId"></param>
    /// <param name="targetId"></param>
    /// <returns>The changed definition, or null if nothing referenced the connection.</returns>
    public static JsonDocument? ReplaceDefinitionReferences(JsonDocument? definition, int llmId, int targetId)
    {
        var root = ToNode(definition);
        if (root == null) return null;
        var holders = DefinitionHolders(root).Where(h => GetId(h.Holder) == llmId).ToArray();
        foreach (var (holder, _) in holders) SetId(holder, targetId);
        return holders.Length == 0 ? null : JsonDocument.Parse(root.ToJsonString());
    }

    /// <summary>
    /// Get the connection report section settings use.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static int? GetSectionLLMId(JsonDocument settings)
        => ToNode(settings) is JsonObject root ? GetId(root) : null;

    /// <summary>
    /// Get the temperature a section must move to in order to be within a range.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns>The new temperature, or null if the section has none or it is within the range.</returns>
    public static float? GetClampedTemperature(JsonDocument settings, float? min, float? max)
    {
        if (ToNode(settings) is not JsonObject root) return null;
        var value = Find(root, TemperatureKey)?.Value;
        if (value == null) return null;
        var element = value.GetValue<JsonElement>();
        float temperature;
        if (element.ValueKind == JsonValueKind.Number) temperature = element.GetSingle();
        else if (element.ValueKind != JsonValueKind.String || !float.TryParse(element.GetString(), System.Globalization.CultureInfo.InvariantCulture, out temperature)) return null;
        if (min.HasValue && temperature < min.Value) return min.Value;
        if (max.HasValue && temperature > max.Value) return max.Value;
        return null;
    }

    /// <summary>
    /// Move report section settings to another connection, keeping its prompts.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="targetId"></param>
    /// <param name="temperature">The temperature to change to, if any.</param>
    /// <returns></returns>
    public static JsonDocument ReplaceSectionLLM(JsonDocument settings, int targetId, float? temperature)
    {
        var root = (JsonObject)ToNode(settings)!;
        SetId(root, targetId);
        if (temperature.HasValue) root[Find(root, TemperatureKey)?.Key ?? TemperatureKey] = temperature.Value;
        return JsonDocument.Parse(root.ToJsonString());
    }

    private static IEnumerable<(JsonObject Holder, string Description)> DefinitionHolders(JsonNode root)
    {
        if (root is not JsonObject definition) yield break;
        foreach (var step in Items(definition, "steps"))
        {
            var stepName = GetText(step, "name") ?? "";
            yield return (step, $"Step '{stepName}'");
            foreach (var analysis in Items(step, "analyses"))
                yield return (analysis, $"Analysis '{GetText(analysis, "name") ?? ""}' in step '{stepName}'");
            foreach (var action in Items(step, "actions"))
                yield return (action, $"Action '{GetText(action, "name") ?? GetText(action, "type") ?? ""}' in step '{stepName}'");
        }
    }

    private static JsonNode? ToNode(JsonDocument? document)
        => document == null ? null : JsonNode.Parse(document.RootElement.GetRawText());

    private static IEnumerable<JsonObject> Items(JsonObject parent, string key)
        => Find(parent, key)?.Value is JsonArray items ? items.OfType<JsonObject>() : Array.Empty<JsonObject>();

    private static KeyValuePair<string, JsonNode?>? Find(JsonObject parent, string key)
    {
        foreach (var property in parent)
            if (String.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase)) return property;
        return null;
    }

    private static string? GetText(JsonObject parent, string key)
        => Find(parent, key)?.Value is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static int? GetId(JsonObject parent)
    {
        if (Find(parent, LLMIdKey)?.Value is not JsonValue value) return null;
        var element = value.GetValue<JsonElement>();
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var number)) return number;
        return element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var parsed) ? parsed : null;
    }

    private static void SetId(JsonObject parent, int id)
        => parent[Find(parent, LLMIdKey)?.Key ?? LLMIdKey] = id;
}
