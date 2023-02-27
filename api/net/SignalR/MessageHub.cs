using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TNO.API.Models.SignalR;

namespace TNO.API.SignalR;

/// <summary>
/// The MessageHub class, provides the SignalR hub to control messages.
/// </summary>
[AllowAnonymous]
[Authorize]
public class MessageHub : Hub
{
    #region Variables
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MessageHub, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    public MessageHub(IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Inform all users or the specified 'username' of the specified 'content' being updated.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task ContentUpdatedAsync(Entities.Content content, string? username = null)
    {
        if (!String.IsNullOrWhiteSpace(username))
            await this.Clients.User(username).SendAsync("Content", new ContentMessageModel(content));
        else
            await this.Clients.All.SendAsync("Content", new ContentMessageModel(content));
    }

    /// <summary>
    /// Informs all users that the specified 'workOrder' has been updated.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task WorkOrderUpdatedAsync(Entities.WorkOrder workOrder)
    {
        await this.Clients.All.SendAsync("WorkOrder", new WorkOrderMessageModel(workOrder, _serializerOptions));
    }
    #endregion
}
