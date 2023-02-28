using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

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
    #endregion
}
