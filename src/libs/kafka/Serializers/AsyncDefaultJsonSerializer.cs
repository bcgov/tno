using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace TNO.Kafka.Serializers;

/// <summary>
/// AsyncDefaultJsonSerializer class, provides a default json serializer.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AsyncDefaultJsonSerializer<T> : IAsyncSerializer<T>, IAsyncDeserializer<T?>
{
    #region Variables
    private readonly JsonSerializerOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instandce of a AsyncDefaultJsonSerializer object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public AsyncDefaultJsonSerializer(IOptions<JsonSerializerOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates a new instandce of a AsyncDefaultJsonSerializer object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public AsyncDefaultJsonSerializer(JsonSerializerOptions options)
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
    public async Task<byte[]> SerializeAsync(T data, SerializationContext context)
    {
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync<T>(ms, data, _options);
        var writer = new StreamWriter(ms);
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
    public async Task<T?> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
    {
        using var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(data);
        writer.Flush();
        ms.Position = 0;
        return !isNull ? await JsonSerializer.DeserializeAsync<T>(ms, _options) : default!;
    }
    #endregion
}
