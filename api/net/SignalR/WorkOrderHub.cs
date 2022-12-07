using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TNO.API.SignalR;

/// <summary>
/// The WorkOrderHub class
/// </summary>
[Authorize]
public class WorkOrderHub : Hub
{
}
