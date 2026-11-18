using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

/// <summary>
/// Converts chat message content into a single string regardless of whether the payload is
/// a plain string or the newer array-of-parts format returned by Azure OpenAI.
/// </summary>
internal sealed class LlmMessageContentConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader),
            _ => ReadFallback(ref reader)
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    private static string? ReadArray(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var builder = new StringBuilder();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var text = ExtractText(element);
            if (string.IsNullOrWhiteSpace(text)) continue;
            if (builder.Length > 0) builder.AppendLine();
            builder.Append(text.Trim());
        }
        return builder.Length == 0 ? null : builder.ToString();
    }

    private static string? ReadObject(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ExtractText(doc.RootElement);
    }

    private static string? ReadFallback(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.GetRawText();
    }

    private static string? ExtractText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object when element.TryGetProperty("text", out var textNode) && textNode.ValueKind == JsonValueKind.String
                => textNode.GetString(),
            JsonValueKind.Object when element.TryGetProperty("content", out var nested) && nested.ValueKind == JsonValueKind.String
                => nested.GetString(),
            JsonValueKind.Object when element.TryGetProperty("type", out var typeNode)
                => ExtractTextFromTypedNode(element, typeNode),
            JsonValueKind.Number => element.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.GetRawText()
        };
    }

    private static string? ExtractTextFromTypedNode(JsonElement element, JsonElement typeNode)
    {
        var type = typeNode.ValueKind == JsonValueKind.String ? typeNode.GetString() : null;
        if (string.Equals(type, "text", StringComparison.OrdinalIgnoreCase) && element.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
            return text.GetString();
        if (string.Equals(type, "tool_result", StringComparison.OrdinalIgnoreCase) && element.TryGetProperty("content", out var nested))
            return nested.ValueKind == JsonValueKind.String ? nested.GetString() : nested.GetRawText();
        return element.GetRawText();
    }
}
