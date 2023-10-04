using System.Security.Claims;
using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;

namespace TNO.API.Helpers;

/// <summary>
/// WorkOrderHelper class, provides methods to help with work orders.
/// </summary>
public class WorkOrderHelper : IWorkOrderHelper
{
    #region Variables
    /// <summary>
    /// The following work order status ensure only a single request can be completed for content.
    /// </summary>
    public static readonly IEnumerable<Entities.WorkOrderStatus> WorkLimiterStatus = new[] { Entities.WorkOrderStatus.Submitted, Entities.WorkOrderStatus.InProgress, Entities.WorkOrderStatus.Completed };

    private readonly ClaimsPrincipal _principal;
    private readonly IContentService _contentService;
    private readonly IWorkOrderService _workOrderService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="contentService"></param>
    /// <param name="workOrderService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    public WorkOrderHelper(
        ClaimsPrincipal principal,
        IContentService contentService,
        IWorkOrderService workOrderService,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _principal = principal;
        _contentService = contentService;
        _workOrderService = workOrderService;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determine if the content should be auto transcribed.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public bool ShouldAutoTranscribe(long contentId)
    {
        var content = _contentService.FindById(contentId);
        return content?.ContentType == Entities.ContentType.AudioVideo &&
            content.FileReferences.Any() &&
            !content.IsApproved &&
            (content.Source?.AutoTranscribe == true ||
                content.Product?.AutoTranscribe == true ||
                content.Contributor?.AutoTranscribe == true ||
                content.Series?.AutoTranscribe == true);
    }

    /// <summary>
    /// Request a transcript for the specified 'contentId'.
    /// Only allow one active transcript request.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    public async Task<Entities.WorkOrder> RequestTranscriptionAsync(long contentId, bool force = false)
    {
        var content = _contentService.FindById(contentId) ?? throw new NoContentException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (force || !workOrders.Any(o => o.WorkType == Entities.WorkOrderType.Transcription || !WorkLimiterStatus.Contains(o.Status)))
        {
            var username = _principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
            var workOrder = _workOrderService.AddAndSave(new Entities.WorkOrder(Entities.WorkOrderType.Transcription, user, "", content));

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.TranscriptionTopic, new TNO.Kafka.Models.TranscriptRequestModel(workOrder));
            return workOrder;
        }
        return workOrders.OrderByDescending(w => w.CreatedOn).First();
    }

    /// <summary>
    /// Request a natural language processing for the specified 'contentId'.
    /// Only allow one active nlp request.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    public async Task<Entities.WorkOrder> RequestNLPAsync(long contentId, bool force = false)
    {
        var content = _contentService.FindById(contentId) ?? throw new NoContentException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        // Only allow one work order nlp request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (force || !workOrders.Any(o => o.WorkType == Entities.WorkOrderType.NaturalLanguageProcess || !WorkLimiterStatus.Contains(o.Status)))
        {
            var username = _principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
            var workOrder = _workOrderService.AddAndSave(new Entities.WorkOrder(Entities.WorkOrderType.NaturalLanguageProcess, user, "", content));

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NLPTopic, new TNO.Kafka.Models.NlpRequestModel(workOrder));
            return workOrder;
        }
        return workOrders.OrderByDescending(w => w.CreatedOn).First();
    }
    #endregion
}
