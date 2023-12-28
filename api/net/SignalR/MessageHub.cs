using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TNO.API.CSS;

namespace TNO.API.SignalR;

/// <summary>
/// The MessageHub class, provides the SignalR hub to control messages.
/// </summary>
[AllowAnonymous]
[Authorize]
public class MessageHub : Hub
{
    #region Variables
    private readonly ICssHelper _cssHelper;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MessageHub, initializes with specified parameters.
    /// </summary>
    /// <param name="cssHelper"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public MessageHub(ICssHelper cssHelper, IOptions<JsonSerializerOptions> serializerOptions, ILogger<MessageHub> logger)
    {
        _cssHelper = cssHelper;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Allows a user to join a group for messages.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HubMethodName("JoinRoom")]
    public async Task JoinRoomAsync(string name)
    {
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, name);
    }

    /// <summary>
    /// Provides way for user to leave a group.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HubMethodName("LeaveRoom")]
    public Task LeaveRoomAsync(string name)
    {
        return this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, name);
    }

    /// <summary>
    /// Returns the message sent
    /// </summary>
    /// <param name="target"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubMethodName("ping")]
    public Task PingAsync(string target, object message)
    {
        return this.Clients.Caller.SendAsync(target, message);
    }

    /// <summary>
    /// Logout all devices connected to one account.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="deviceKey"></param>
    /// <returns></returns>
    [HubMethodName("logout")]
    public Task LogoutAsync(string username, string? deviceKey = null)
    {
        if (this.Context.User != null)
            _cssHelper.RemoveOtherLocations(this.Context.User, deviceKey);
        return this.Clients.User(username).SendAsync("Logout");
    }
    #endregion
}
