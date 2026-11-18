using Microsoft.AspNetCore.SignalR;
using TNO.Core.Extensions;

namespace TNO.Kafka.SignalR;

/// <summary>
/// HubUsernameProvider class, provides a way to extract the username to uniquely identify the user.
/// </summary>
public class HubUsernameProvider : IUserIdProvider
{
    /// <summary>
    /// Extract the username from the connection.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.GetUsername();
    }
}
