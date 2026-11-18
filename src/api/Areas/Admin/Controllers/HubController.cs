using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// HubController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/hub")]
[Route("api/[area]/hub")]
[Route("v{version:apiVersion}/[area]/hub")]
[Route("[area]/hub")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class HubController : ControllerBase
{
    #region Variables
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HubController object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    public HubController(
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions)
    {
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Send message signal to hub.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="target"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Hub" })]
    public async Task<IActionResult> AddAsync(HubEvent hubEvent, MessageTarget target, object message)
    {
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(hubEvent, new KafkaInvocationMessage(target, new[] { message })));

        return new OkResult();
    }
    #endregion
}
