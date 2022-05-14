using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace TNO.Services.Serializers;

/// <summary>
/// DefaultJsonSerializer class, provides a default json serializer.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultJsonSerializer<T> : ISerializer<T>, IDeserializer<T?>
{
    #region Variables
    private readonly JsonSerializerOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instandce of a DefaultJsonSerializer object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public DefaultJsonSerializer(IOptions<JsonSerializerOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates a new instandce of a DefaultJsonSerializer object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public DefaultJsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Serialize the data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public byte[] Serialize(T data, SerializationContext context)
    {
        var json = JsonSerializer.Serialize<T>(data, _options);
        using var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(json);
        writer.Flush();
        ms.Position = 0;
        return ms.ToArray();
    }

    /// <summary>
    /// Deserialize the data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isNull"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public T? Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return !isNull ? JsonSerializer.Deserialize<T>(data.ToArray()) : default;
    }
    #endregion
}
