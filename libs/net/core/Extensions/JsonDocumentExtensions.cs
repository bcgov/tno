using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    /// <summary>
    /// Get the JsonElement for the specified 'path'.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static JsonElement? GetElement(this JsonDocument doc, string path)
    {
        return JsonElementExtensions.GetElement(doc.RootElement, path);
    }

    /// <summary>
    /// Get the value for the specified 'path'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="path"></param>
    /// <param name="defaultValue"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T? GetElementValue<T>(this JsonDocument doc, string path = "", T? defaultValue = default, JsonSerializerOptions? options = null)
    {
        return JsonElementExtensions.GetElementValue(doc.RootElement, path, defaultValue, options);
    }

    /// <summary>
    /// Clone the node.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <param name="node"></param>
    /// <returns></returns>
    public static TNode? CopyNode<TNode>(this TNode? node) where TNode : JsonNode => node?.Deserialize<TNode>();

    /// <summary>
    /// Move the array to the new parent.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="id"></param>
    /// <param name="newParent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static JsonNode? MoveNode(this JsonArray array, int id, JsonObject newParent, string name)
    {
        var node = array[id];
        array.RemoveAt(id);
        return newParent[name] = node;
    }

    /// <summary>
    /// Move the object to the new parent.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="oldName"></param>
    /// <param name="newParent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static JsonNode? MoveNode(this JsonObject parent, string oldName, JsonObject newParent, string name)
    {
        parent.Remove(oldName, out var node);
        return newParent[name] = node;
    }

    /// <summary>
    /// Throw exception if the node is null.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public static TNode ThrowOnNull<TNode>(this TNode? value) where TNode : JsonNode => value ?? throw new JsonException("Null JSON value");
}
