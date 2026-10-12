using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ReportAISectionTestController class, receives the outcome of AI section test runs from the
/// reporting service and hands it to the user who requested the run.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/ai-tests")]
[Route("api/[area]/reports/ai-tests")]
[Route("v{version:apiVersion}/[area]/reports/ai-tests")]
[Route("[area]/reports/ai-tests")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportAISectionTestController : ControllerBase
{
    #region Variables
    private readonly IReportAISectionTestStore _store;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAISectionTestController object, initializes with specified parameters.
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    public ReportAISectionTestController(
        IReportAISectionTestStore store,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions)
    {
        _store = store;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Record the outcome of a test run and tell the requestor it is ready. The message on the
    /// report status channel carries "ai-test:{testId}" so an open workbench can fetch it.
    /// </summary>
    /// <param name="testId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("{testId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportAIResult" })]
    public async Task<IActionResult> CompleteAsync(Guid testId, [FromBody] ReportAISectionTestResultModel model)
    {
        model.TestId = testId;
        if (model.Status == ReportAISectionTestStatus.Pending) model.Status = ReportAISectionTestStatus.Failed;
        var result = _store.Complete(model);

        var user = result.RequestorId.HasValue ? _userService.FindById(result.RequestorId.Value) : null;
        if (user != null)
        {
            var message = new ReportMessageModel()
            {
                Id = result.ReportInstanceId,
                ReportId = result.ReportId,
                Message = $"ai-test:{result.TestId}",
            };
            await _kafkaMessenger.SendMessageAsync(
                _kafkaHubOptions.HubTopic,
                new KafkaHubMessage(HubEvent.SendUser, user.Username, new KafkaInvocationMessage(MessageTarget.ReportStatus, new[] { message }))
            );
        }
        return Ok();
    }
    #endregion
}
