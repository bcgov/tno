using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Topic;

/// <summary>
/// TopicPopulationSettingsModel class, how Content-Analysis assigns topics to content (when the
/// service runs its Topics process).
/// </summary>
public class TopicPopulationSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Assign existing active topics only (default), or allow creating topics.
    /// </summary>
    public TopicPopulationMode Mode { get; set; } = TopicPopulationMode.ExistingOnly;
    #endregion
}
