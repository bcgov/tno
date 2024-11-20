using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Kafka.Models;
using TNO.API.Config;
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
    private readonly KafkaOptions _kafkaOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProducerController object, initializes with specified parameters.
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="kafkaOptions"></param>
    public ProducerController(IKafkaMessenger producer, IOptions<KafkaOptions> kafkaOptions)
    {
        _producer = producer;
        _kafkaOptions = kafkaOptions.Value;
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
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DeliveryResultModel<SourceContent>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Kafka" })]
    public async Task<IActionResult> SendContentAsync(string topic, [FromBody] SourceContent model)
    {
        var result = (await _producer.SendMessageAsync(topic, model)) ?? throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
        return new JsonResult(new DeliveryResultModel<SourceContent>(result))
        {
            StatusCode = 201
        };
    }

    /// <summary>
    /// Publish a notification request message to Kafka.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("notification")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DeliveryResultModel<NotificationRequestModel>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Kafka" })]
    public async Task<IActionResult> SendNotificationAsync([FromBody] NotificationRequestModel model)
    {
        var result = (await _producer.SendMessageAsync(_kafkaOptions.NotificationTopic, model)) ?? throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
        return new JsonResult(new DeliveryResultModel<NotificationRequestModel>(result))
        {
            StatusCode = 201
        };
    }

    /// <summary>
    /// Publish a notification request message to Kafka.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("report")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DeliveryResultModel<ReportRequestModel>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Kafka" })]
    public async Task<IActionResult> SendReportAsync([FromBody] ReportRequestModel model)
    {
        var result = (await _producer.SendMessageAsync(_kafkaOptions.ReportingTopic, model)) ?? throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
        return new JsonResult(new DeliveryResultModel<ReportRequestModel>(result))
        {
            StatusCode = 201
        };
    }

    /// <summary>
    /// Publish a scheduled event request message to Kafka.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("event")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DeliveryResultModel<EventScheduleRequestModel>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Kafka" })]
    public async Task<IActionResult> SendEventAsync([FromBody] EventScheduleRequestModel model)
    {
        var result = (await _producer.SendMessageAsync(_kafkaOptions.EventTopic, model)) ?? throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
        return new JsonResult(new DeliveryResultModel<EventScheduleRequestModel>(result))
        {
            StatusCode = 201
        };
    }
    #endregion
}
