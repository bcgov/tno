using System.Text.Json;

namespace TNO.Services.Notification.Models;

/// <summary>
/// NotificationRetryModel class, the state a notification request carries when it is sent back to Kafka
/// because emails could not be handed to CHES.
/// </summary>
public class NotificationRetryModel
{
    #region Variables
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    #endregion

    #region Properties
    /// <summary>
    /// get/set - Number of attempts that have failed for this notification and content.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// get/set - The subscribers whose email failed.  Only these users are sent the retry.
    /// </summary>
    public int[] UserIds { get; set; } = [];

    /// <summary>
    /// get/set - Earliest time (UTC) the retry should be attempted.
    /// </summary>
    public DateTime? RetryAfter { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Extract the retry state from the 'data' of a notification request.
    /// </summary>
    /// <param name="data">The request data, a JsonElement when it arrived from Kafka.</param>
    /// <returns>The retry state, or null when the request is not a retry.</returns>
    public static NotificationRetryModel? From(object? data)
    {
        if (data == null) return null;
        try
        {
            var element = data is JsonElement json ? json : JsonSerializer.SerializeToElement(data, _serializerOptions);
            if (element.ValueKind != JsonValueKind.Object) return null;

            var retry = element.Deserialize<NotificationRetryModel>(_serializerOptions);
            return retry?.FailureCount > 0 ? retry : null;
        }
        catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
        {
            // The data belongs to some other kind of request.
            return null;
        }
    }

    /// <summary>
    /// Serialize the retry state so that it can be placed in the 'data' of a notification request.
    /// </summary>
    /// <returns></returns>
    public JsonElement ToData()
    {
        return JsonSerializer.SerializeToElement(this, _serializerOptions);
    }
    #endregion
}
