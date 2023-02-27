using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.API.SignalR;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Keycloak;
using TNO.Models.Extensions;

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
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly MessageHub _hub;
    private readonly ApiOptions _apiOptions;
    private readonly JsonSerializerOptions _serializerOptions;

    // The following work order status ensure only a single request can be completed for content.
    private readonly IEnumerable<WorkOrderStatus> _workLimiterStatus = new[] { WorkOrderStatus.Submitted, WorkOrderStatus.InProgress };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderService"></param>
    /// <param name="contentService"></param>
    /// <param name="userService"></param>
    /// <param name="connection"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="hub"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="apiOptions"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(
        IWorkOrderService workOrderService,
        IContentService contentService,
        IUserService userService,
        IConnectionHelper connection,
        IKafkaMessenger kafkaMessenger,
        MessageHub hub,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<ApiOptions> apiOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _workOrderService = workOrderService;
        _contentService = contentService;
        _userService = userService;
        _connection = connection;
        _kafkaMessenger = kafkaMessenger;
        _hub = hub;
        _kafkaOptions = kafkaOptions.Value;
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
    [ProducesResponseType(typeof(IPaged<WorkOrderMessageModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _workOrderService.Find(new WorkOrderFilter(query));
        var page = new Paged<WorkOrderMessageModel>(result.Items.Select(i => new WorkOrderMessageModel(i, _serializerOptions)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> RequestTranscriptionAsync(long contentId)
    {
        var content = _contentService.FindById(contentId) ?? throw new InvalidOperationException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (workOrders.Any(o => o.WorkType == WorkOrderType.Transcription && _workLimiterStatus.Contains(o.Status)))
            return new JsonResult(new WorkOrderMessageModel(workOrders.First(o => o.WorkType == WorkOrderType.Transcription && _workLimiterStatus.Contains(o.Status)), _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var workOrder = _workOrderService.AddAndSave(new WorkOrder(WorkOrderType.Transcription, user, "", content));

        var result = await _kafkaMessenger.SendMessageAsync(_kafkaOptions.TranscriptionTopic, new TNO.Kafka.Models.TranscriptRequestModel(workOrder));
        if (result == null)
        {
            workOrder.Status = WorkOrderStatus.Failed;
            workOrder.Note = "Transcript request to Kafka failed";
            workOrder = _workOrderService.UpdateAndSave(workOrder);
            await _hub.WorkOrderUpdatedAsync(workOrder);
            throw new BadRequestException("Transcription request failed");
        }
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> RequestNLPAsync(long contentId)
    {
        var content = _contentService.FindById(contentId) ?? throw new InvalidOperationException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.NLPTopic)) throw new ConfigurationException("Kafka NLP topic not configured.");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (workOrders.Any(o => o.WorkType == WorkOrderType.NaturalLanguageProcess && _workLimiterStatus.Contains(o.Status)))
            return new JsonResult(new WorkOrderMessageModel(workOrders.First(o => o.WorkType == WorkOrderType.NaturalLanguageProcess && _workLimiterStatus.Contains(o.Status)), _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var workOrder = _workOrderService.AddAndSave(new WorkOrder(WorkOrderType.NaturalLanguageProcess, user, "", content));

        var result = await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NLPTopic, new TNO.Kafka.Models.NlpRequestModel(workOrder));
        if (result == null)
        {
            workOrder.Status = WorkOrderStatus.Failed;
            workOrder.Note = "NLP request to Kafka failed";
            workOrder = _workOrderService.UpdateAndSave(workOrder);
            await _hub.WorkOrderUpdatedAsync(workOrder);
            throw new BadRequestException("Natural Language Processing request failed");
        }
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
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
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
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var exists = client.Exists(Path.Combine(locationPath, path));
                    if (!exists) throw new BadRequestException("File does not exist.");

                    // Only allow one work order transcript request at a time.
                    // TODO: Handle blocked work orders stuck in progress.
                    var workOrders = _workOrderService.FindByFile(locationId, path);
                    if (workOrders.Any(o => o.WorkType == WorkOrderType.FileRequest && _workLimiterStatus.Contains(o.Status)))
                        return new JsonResult(new WorkOrderMessageModel(workOrders.First(o => o.WorkType == WorkOrderType.FileRequest && _workLimiterStatus.Contains(o.Status)), _serializerOptions))
                        {
                            StatusCode = (int)HttpStatusCode.AlreadyReported
                        };

                    var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
                    var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
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
    #endregion
}
