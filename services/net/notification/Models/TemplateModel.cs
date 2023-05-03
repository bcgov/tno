using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Services.Notification.Config;

namespace TNO.Services.Notification.Models
{
    public class TemplateModel
    {
        public TemplateModel(IOptions<NotificationOptions> notificationOptions)
        {
            NotificationOptions = notificationOptions;
        }

        /// <summary>
        /// get/set - The content model.
        /// </summary>
        public ContentModel Content { get; set; } = new ContentModel();

        /// <summary>
        /// get - Notification options.
        /// </summary>
        public IOptions<NotificationOptions> NotificationOptions { get; }
    }
}
