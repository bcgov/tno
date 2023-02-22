using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TNO.API.SignalR;

/// <summary>
/// The WorkOrderHub class, provides the SignalR hub to control messages.
/// </summary>
[AllowAnonymous]
[Authorize]
public class WorkOrderHub : Hub
{
}
