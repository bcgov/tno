using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.Notification;

namespace TNO.Services;

/// <summary>
/// NotificationValidator class, provides a way to validate whether a notification should be executed for the specified content.
/// </summary>
public class NotificationValidator : Entities.Validation.NotificationValidator
{
    #region Properties
    /// <summary>
    /// get - The API service.
    /// </summary>
    protected IApiService Api { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationValidator object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    public NotificationValidator(IApiService api)
    {
        this.Api = api;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initializes properties and make sure Content.Instance has been populated.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="alertId"></param>
    public async Task InitializeAsync(NotificationModel notification, ContentModel content, int alertId)
    {
        content.Notifications = await this.Api.GetNotificationsForAsync(content.Id);
        base.Initialize((Entities.Notification)notification, (Entities.Content)content, alertId);
    }
    #endregion
}
