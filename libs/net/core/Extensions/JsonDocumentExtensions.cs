using System.Text;
using System.Text.Json;

namespace TNO.Core.Extensions;

/// <summary>
/// JsonDocumentExtensions static class, provides extension methods for JsonDocument objects.
/// </summary>
public static class JsonDocumentExtensions
{
    /// <summary>
    /// Serialize the specified 'doc'.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToJson(this JsonDocument doc, JsonWriterOptions? options = null)
    {
        options ??= new JsonWriterOptions { Indented = true };
        using var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, options.Value);
        doc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
