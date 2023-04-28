using TNO.API.Areas.Services.Models.Content;

namespace TNO.Services.Notification.Models
{
    public class TemplateModel
    {
        /// <summary>
        /// get/set - The content model.
        /// </summary>
        public ContentModel Content { get; set; } = new ContentModel();
    }
}
