using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.WorkOrder;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Extensions;
using TNO.Models.Filters;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// WorkOrderController class, provides WorkOrder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/work/orders")]
[Route("api/[area]/work/orders")]
[Route("v{version:apiVersion}/[area]/work/orders")]
[Route("[area]/work/orders")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class WorkOrderController : ControllerBase
{
    #region Variables
    private readonly IConnectionHelper _connection;
    private readonly IWorkOrderService _workOrderService;
    private readonly IUserService _userService;
    private readonly IWorkOrderHelper _workOrderHelper;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly ApiOptions _apiOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderService"></param>
    /// <param name="userService"></param>
    /// <param name="workOrderHelper"></param>
    /// <param name="connection"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="apiOptions"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(
        IWorkOrderService workOrderService,
        IUserService userService,
        IWorkOrderHelper workOrderHelper,
        IConnectionHelper connection,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<ApiOptions> apiOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _workOrderService = workOrderService;
        _userService = userService;
        _workOrderHelper = workOrderHelper;
        _connection = connection;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _apiOptions = apiOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of work orders for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<WorkOrderModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _workOrderService.FindDistinctWorkOrders(new WorkOrderFilter(query), this._serializerOptions);
        var page = new Paged<WorkOrderModel>(result.Items.Select(i => i), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Update work order for the specified 'id'.
    /// Update the work order in Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> UpdateAsync(WorkOrderModel model)
    {
        var entity = _workOrderService.FindById(model.Id) ?? throw new NoContentException();
        var result = _workOrderService.UpdateAndSave(model.CopyTo(entity, _serializerOptions));
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.WorkOrder, new[] { new WorkOrderMessageModel(result, _serializerOptions) })));
        return new JsonResult(new WorkOrderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Request a transcript for the content for the specified 'contentId'.
    /// Publish message to kafka to request a transcription.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("transcribe/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderMessageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> RequestTranscriptionAsync(long contentId)
    {
        var workOrder = await _workOrderHelper.RequestTranscriptionAsync(contentId, true);
        if (workOrder.Status != WorkOrderStatus.Submitted)
            return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions));
    }

    /// <summary>
    /// Request a Natural Language Processing for the content for the specified 'contentId'.
    /// Publish message to kafka to request a NLP.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("nlp/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderMessageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> RequestNLPAsync(long contentId)
    {
        var workOrder = await _workOrderHelper.RequestNLPAsync(contentId, true);
        if (workOrder.Status != WorkOrderStatus.Submitted)
            return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions));
    }

    /// <summary>
    /// Make a work order request for a remote file so that it can be copied to a volume local to the API.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPost("request/file/{locationId:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> RequestFileAsync([FromRoute] int locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? throw new BadRequestException("Query parameter 'path' required.") : HttpUtility.UrlDecode(path).MakeRelativePath();
        if (String.IsNullOrWhiteSpace(Path.GetExtension(path))) throw new BadRequestException("File does not exist.");
        var dataLocation = _connection.GetDataLocation(locationId);

        if (dataLocation?.Connection != null && dataLocation?.Connection.Name != _apiOptions.DataLocation)
        {
            // Validate file exists.
            // TODO: Change SSH to SFTP
            if (dataLocation?.Connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(dataLocation.Connection);
                var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var exists = client.Exists(Path.Combine(locationPath, path));
                    if (!exists) throw new BadRequestException("File does not exist.");

                    // Only allow one work order transcript request at a time.
                    // TODO: Handle blocked work orders stuck in progress.
                    var workOrders = _workOrderService.FindByFile(locationId, path);
                    if (workOrders.Any(o => o.WorkType == WorkOrderType.FileRequest && WorkOrderHelper.WorkLimiterStatus.Contains(o.Status)))
                        return new JsonResult(new WorkOrderMessageModel(workOrders.First(o => o.WorkType == WorkOrderType.FileRequest && WorkOrderHelper.WorkLimiterStatus.Contains(o.Status)), _serializerOptions))
                        {
                            StatusCode = (int)HttpStatusCode.AlreadyReported
                        };

                    var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
                    var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
                    var workOrder = _workOrderService.AddAndSave(new WorkOrder(WorkOrderType.FileRequest, user, "", $"{{ \"locationId\": {locationId}, \"path\":\"{path}\"}}"));

                    var result = await _kafkaMessenger.SendMessageAsync(_kafkaOptions.FileRequestTopic, new TNO.Kafka.Models.FileRequestModel(workOrder));
                    if (result == null)
                    {
                        workOrder.Status = WorkOrderStatus.Failed;
                        workOrder.Note = "Remote file request to Kafka failed";
                        workOrder = _workOrderService.UpdateAndSave(workOrder);
                        throw new BadRequestException("Remote file request failed");
                    }
                    return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions));
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation?.Connection?.ConnectionType}' not implemented yet.");
        }

        throw new BadRequestException("Location is not valid");
    }

    /// <summary>
    /// Request a transcript for the content for the specified 'contentId'.
    /// Publish message to kafka to request a transcription.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("ffmpeg/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderMessageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> RequestFFmpegAsync(long contentId)
    {
        var workOrder = await _workOrderHelper.RequestFFmpegAsync(contentId, true);
        if (workOrder.Status != WorkOrderStatus.Submitted)
            return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        return new JsonResult(new WorkOrderMessageModel(workOrder, _serializerOptions));
    }
    #endregion
}
