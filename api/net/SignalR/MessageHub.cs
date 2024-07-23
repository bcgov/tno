using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TNO.API.Keycloak;

namespace TNO.API.SignalR;

/// <summary>
/// The MessageHub class, provides the SignalR hub to control messages.
/// </summary>
[AllowAnonymous]
[Authorize]
public class MessageHub : Hub
{
    #region Variables
    private readonly IKeycloakHelper _keycloakHelper;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MessageHub, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakHelper"></param>
    public MessageHub(IKeycloakHelper keycloakHelper)
    {
        _keycloakHelper = keycloakHelper;
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
            _keycloakHelper.RemoveOtherLocations(this.Context.User, deviceKey);
        return this.Clients.User(username).SendAsync("Logout");
    }
    #endregion
}
