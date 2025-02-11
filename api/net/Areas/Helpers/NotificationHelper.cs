
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Notifications;

namespace TNO.API.Helpers;

/// <summary>
/// NotificationHelper class, provides helper methods to generate notifications.
/// </summary>
public class NotificationHelper : INotificationHelper
{
    #region Variables
    private readonly INotificationEngine _notificationEngine;
    private readonly INotificationService _notificationService;
    private readonly IContentService _contentService;
    private readonly ElasticOptions _elasticOptions;
    private readonly StorageOptions _storageOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationEngine"></param>
    /// <param name="notificationService"></param>
    /// <param name="overviewTemplateService"></param>
    /// <param name="contentService"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    public NotificationHelper(
        INotificationEngine notificationEngine,
        INotificationService notificationService,
        IAVOverviewTemplateService overviewTemplateService,
        IContentService contentService,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _notificationEngine = notificationEngine;
        _notificationService = notificationService;
        _contentService = contentService;
        _elasticOptions = elasticOptions.Value;
        _storageOptions = storageOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate an instance of the notification for each content item returned by the filter.
    /// If you already have a content item, then manually create the NotificationInstance(id, contentId, requestorId)
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Entities.NotificationInstance>> GenerateNotificationInstancesAsync(
        Areas.Services.Models.Notification.NotificationModel model,
        int? requestorId = null)
    {
        var searchResults = await _notificationService.FindContentWithElasticsearchAsync(model.ToEntity(_serializerOptions), requestorId);
        return searchResults.Hits.Hits
            .Select(h => h.Source)
            .Where(c => c != null)
            .Select(c => new Entities.NotificationInstance(model.Id, c.Id, requestorId))
            .ToArray();
    }

    /// <summary>
    /// Execute the notification template to generate the subject and body.
    /// Uses the content already in the notification instance.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <param name="enableReportSentiment"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<NotificationResultModel> GenerateNotificationAsync(
        Areas.Services.Models.NotificationInstance.NotificationInstanceModel model,
        bool isPreview = false,
        bool enableReportSentiment = false)
    {
        var notification = _notificationService.FindById(model.NotificationId) ?? throw new ArgumentException("Parameter 'model.NotificationId' did not return a notification.");
        if (notification.Template == null) throw new InvalidOperationException("Parameter 'Notification.Template' is required");

        var content = _contentService.FindById(model.ContentId) ?? throw new NoContentException($"Content '{model.ContentId}' does not exist");
        var notificationModel = new Areas.Services.Models.Notification.NotificationModel(notification, _serializerOptions);
        var contentModel = new ContentModel(content);

        return await GenerateNotificationAsync(notificationModel, contentModel, isPreview);
    }

    /// <summary>
    /// Execute the notification template to generate the subject and body.
    /// Fetch content from elasticsearch.
    /// Use this method for preview.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IEnumerable<NotificationResultModel>> GenerateNotificationsAsync(
        Areas.Services.Models.Notification.NotificationModel model,
        int? requestorId = null,
        bool isPreview = false)
    {
        if (model.Template == null) throw new ArgumentException("Parameter 'model.Template' is required");

        // Fetch all content for this notification.
        var elasticResults = await _notificationService.FindContentWithElasticsearchAsync(model.ToEntity(_serializerOptions), requestorId);

        var results = new List<NotificationResultModel>();
        foreach (var content in elasticResults.Hits.Hits.Select(h => h.Source).Where(c => c != null))
        {
            var contentModel = new ContentModel(content);
            var result = await GenerateNotificationAsync(model, contentModel, isPreview);
            result.Data = elasticResults;
            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="isPreview"></param>
    /// <param name="enableReportSentiment"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<NotificationResultModel> GenerateNotificationAsync(
        Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        bool isPreview = false,
        bool enableReportSentiment = false)
    {
        var subject = await _notificationEngine.GenerateNotificationSubjectAsync(notification, content, isPreview, enableReportSentiment);
        var body = await _notificationEngine.GenerateNotificationBodyAsync(notification, content, _storageOptions.GetUploadPath(), isPreview);

        return new NotificationResultModel(subject, body);
    }
    #endregion
}
