using TNO.API.Areas.Services.Models.Content;

namespace TNO.Services.Notification.Models
{
    public class TemplateModel
    {
        /// <summary>
        /// get/set - The content model.
        /// </summary>
        public ContentModel Content { get; set; } = new ContentModel();

        /// <summary>
        /// get/set - The MMIA URL.
        /// </summary>
        public string? MmiaUrl { get; set; }

        /// <summary>
        /// get/set - The request transcript URL.
        /// </summary>
        public string? RequestTranscriptUrl { get; set; }

        /// <summary>
        /// get/set - The add to report URL.
        /// </summary>
        public string? AddToReportUrl { get; set; }
    }
}
