using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.API.Models.Settings;
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
    public static readonly Entities.WorkOrderStatus[] WorkLimiterStatus = new[] { Entities.WorkOrderStatus.Submitted, Entities.WorkOrderStatus.InProgress, Entities.WorkOrderStatus.Completed };

    private readonly ClaimsPrincipal _principal;
    private readonly IContentService _contentService;
    private readonly IWorkOrderService _workOrderService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    /// <summary>
    /// get/set - The content
    /// </summary>
    public Entities.Content? Content { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="contentService"></param>
    /// <param name="workOrderService"></param>
    /// <param name="userService"></param>
    /// <param name="notificationService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderHelper(
        ClaimsPrincipal principal,
        IContentService contentService,
        IWorkOrderService workOrderService,
        IUserService userService,
        INotificationService notificationService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _principal = principal;
        _contentService = contentService;
        _workOrderService = workOrderService;
        _userService = userService;
        _notificationService = notificationService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the FFmpeg actions configured for the specified mediaType.
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    private IEnumerable<FFmpegActionSettingsModel> GetFFmpegActions(Entities.MediaType? mediaType)
    {
        if (mediaType == null) return Array.Empty<FFmpegActionSettingsModel>();
        var settings = JsonSerializer.Deserialize<MediaTypeSettingsModel>(mediaType.Settings, _serializerOptions);
        return settings?.FFmpeg ?? Array.Empty<FFmpegActionSettingsModel>();
    }

    /// <summary>
    /// Determine if the content should perform FFmpeg actions.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public bool ShouldFFmpeg(long contentId)
    {
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId);

        return this.Content?.ContentType == Entities.ContentType.AudioVideo &&
            this.Content.FileReferences.Any() &&
            GetFFmpegActions(this.Content.MediaType).Any();
    }

    /// <summary>
    /// Determine if the content should be auto transcribed.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public bool ShouldAutoTranscribe(long contentId)
    {
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId);
        return this.Content?.ContentType == Entities.ContentType.AudioVideo &&
            this.Content.FileReferences.Any() &&
            !this.Content.IsApproved &&
            (this.Content.Source?.AutoTranscribe == true ||
                this.Content.MediaType?.AutoTranscribe == true ||
                this.Content.Contributor?.AutoTranscribe == true ||
                this.Content.Series?.AutoTranscribe == true);
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
        string username = _principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User is missing");

        return await RequestTranscriptionAsync(contentId, user, force);
    }

    /// <summary>
    /// Determine if the content has an existing transcript.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public bool HasExistingTranscript(long contentId)
    {
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId);

        return this.Content?.ContentType == Entities.ContentType.AudioVideo &&
               !string.IsNullOrWhiteSpace(this.Content.Body);
    }

    /// <summary>
    /// Request a transcript for the specified 'contentId'.
    /// Only allow one active transcript request.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="requestor"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Entities.WorkOrder> RequestTranscriptionAsync(long contentId, Entities.User requestor, bool force = false)
    {
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId) ?? throw new NoContentException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        if (this.Content.IsApproved && force == false) throw new InvalidOperationException("Content is already approved");
        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);

        // Add the user to the content notification.
        _notificationService.SubscriberUserToContent(requestor.Id, contentId);

        if (force || !workOrders.Any(o => o.WorkType == Entities.WorkOrderType.Transcription || !WorkLimiterStatus.Contains(o.Status)))
        {
            var headlineString = $"{{ \"headline\": \"{this.Content.Headline.Replace("\n", "")}\" }}";
            var configuration = JsonDocument.Parse(headlineString);
            var workOrder = _workOrderService.AddAndSave(
                new Entities.WorkOrder(
                    Entities.WorkOrderType.Transcription,
                    requestor,
                    "",
                    this.Content,
                    configuration
                    ));

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
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId) ?? throw new NoContentException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        // Only allow one work order nlp request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (force || !workOrders.Any(o => o.WorkType == Entities.WorkOrderType.NaturalLanguageProcess || !WorkLimiterStatus.Contains(o.Status)))
        {
            var username = _principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
            var workOrder = _workOrderService.AddAndSave(
                new Entities.WorkOrder(
                    Entities.WorkOrderType.NaturalLanguageProcess,
                    user,
                    "",
                    this.Content));

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NLPTopic, new TNO.Kafka.Models.NlpRequestModel(workOrder));
            return workOrder;
        }
        return workOrders.OrderByDescending(w => w.CreatedOn).First();
    }

    /// <summary>
    /// Request a FFmpeg for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    public async Task<Entities.WorkOrder> RequestFFmpegAsync(long contentId, bool force = false)
    {
        if (this.Content == null || this.Content.Id != contentId)
            this.Content = _contentService.FindById(contentId) ?? throw new NoContentException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.FFmpegTopic)) throw new ConfigurationException("Kafka FFmpeg topic not configured.");

        var username = _principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (force || !workOrders.Any(o => o.WorkType == Entities.WorkOrderType.FFmpeg || !WorkLimiterStatus.Contains(o.Status)))
        {
            var workOrder = _workOrderService.AddAndSave(
                new Entities.WorkOrder(
                    Entities.WorkOrderType.FFmpeg,
                    user,
                    "",
                    this.Content,
                    this.Content.MediaType?.Settings));
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.FFmpegTopic, new TNO.Kafka.Models.FFmpegRequestModel(workOrder, _serializerOptions));
            return workOrder;
        }
        return workOrders.OrderByDescending(w => w.CreatedOn).First();
    }
    #endregion
}
