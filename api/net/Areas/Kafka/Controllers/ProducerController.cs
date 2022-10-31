using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Kafka.Models;
using TNO.API.Models;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ProducerController class, provides Kafka producer endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("kafka")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/producers")]
[Route("api/[area]/producers")]
[Route("v{version:apiVersion}/[area]/producers")]
[Route("[area]/producers")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ProducerController : ControllerBase
{
    #region Variables
    private readonly IKafkaMessenger _producer;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProducerController object, initializes with specified parameters.
    /// </summary>
    /// <param name="producer"></param>
    public ProducerController(IKafkaMessenger producer)
    {
        _producer = producer;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Publish a source content message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("content/{topic}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DeliveryResultModel<SourceContent>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public async Task<IActionResult> SendAsync(string topic, SourceContent model)
    {
        var result = await _producer.SendMessageAsync(topic, model);
        if (result == null) throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
        return new JsonResult(new DeliveryResultModel<SourceContent>(result));
    }
    #endregion
}
