
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.TemplateEngine.Config;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Notifications;

namespace TNO.TemplateEngine;

/// <summary>
/// NotificationEngine class, provides a centralize collection of methods to generate notifications and charts.
/// </summary>
public class NotificationEngine : INotificationEngine
{
    #region Properties
    /// <summary>
    /// get - Notification template engine for content.
    /// </summary>
    protected ITemplateEngine<NotificationEngineContentModel> NotificationEngineContent { get; }

    /// <summary>
    /// get - HTTP client.
    /// </summary>
    protected IHttpRequestClient HttpClient { get; }

    /// <summary>
    /// get - Template options.
    /// </summary>
    protected TemplateOptions TemplateOptions { get; }

    /// <summary>
    ///  logger
    /// </summary>
    protected ILogger<NotificationEngine> Logger { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationEngineContent"></param>
    /// <param name="httpClient"></param>
    /// <param name="templateOptions"></param>
    /// <param name="logger"></param>
    public NotificationEngine(
        ITemplateEngine<NotificationEngineContentModel> notificationEngineContent,
        IHttpRequestClient httpClient,
        IOptions<TemplateOptions> templateOptions,
        ILogger<NotificationEngine> logger)
    {
        this.NotificationEngineContent = notificationEngineContent;
        this.HttpClient = httpClient;
        this.TemplateOptions = templateOptions.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateNotificationSubjectAsync(
        API.Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        bool isPreview = false,
        bool enableReportSentiment = false)
    {
        if (notification.Template == null) throw new InvalidOperationException("Notification template is missing from model");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-notification-template-{notification.Template.Id}-subject";
        var template = this.NotificationEngineContent.GetOrAddTemplateInMemory(key, notification.Template.Subject)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new NotificationEngineContentModel(content);
        return await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Content = model.Content;
            instance.EnableReportSentiment = enableReportSentiment;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });
    }

    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="uploadPath"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateNotificationBodyAsync(
        API.Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        string? uploadPath = null,
        bool isPreview = false)
    {
        if (notification.Template == null) throw new InvalidOperationException("Notification template is missing from model");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-notification-template-{notification.Template.Id}-body";
        var template = this.NotificationEngineContent.GetOrAddTemplateInMemory(key, notification.Template.Body)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new NotificationEngineContentModel(content, uploadPath);
        var body = await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Content = model.Content;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });

        return body;
    }
    #endregion
}
