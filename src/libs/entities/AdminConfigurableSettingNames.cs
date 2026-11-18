using TNO.Core.Json;

namespace TNO.Entities;

/// <summary>
/// Used to hold names of settings that can be configured in th Editor > Settings UI.
/// </summary>
public enum AdminConfigurableSettingNames
{
    /// <summary>
    /// Email for the admin responsible for processing report subscription requests.
    /// </summary>
    [EnumValue("ProductSubscriptionManagerEmail")]
    ProductSubscriptionManagerEmail,

    /// <summary>
    /// Days report history (report instances, evening overview instances, cached AI results) is kept. Zero disables the purge.
    /// </summary>
    [EnumValue("ReportRetentionDays")]
    ReportRetentionDays,

    /// <summary>
    /// Days notification history is kept. Zero disables the purge.
    /// </summary>
    [EnumValue("NotificationRetentionDays")]
    NotificationRetentionDays,

    /// <summary>
    /// How Content-Analysis assigns topics: 'ExistingOnly' or 'AllowCreate'.
    /// </summary>
    [EnumValue("TopicPopulationMode")]
    TopicPopulationMode,

    /// <summary>
    /// The LLM (llm table ID) Content-Analysis uses.
    /// </summary>
    [EnumValue("ContentAnalysisLLMId")]
    ContentAnalysisLLMId,

    /// <summary>
    /// Comma-separated media type IDs Content-Analysis does not analyze.
    /// </summary>
    [EnumValue("ContentAnalysisExcludedMediaTypeIds")]
    ContentAnalysisExcludedMediaTypeIds,

    /// <summary>
    /// Comma-separated source IDs Content-Analysis does not analyze.
    /// </summary>
    [EnumValue("ContentAnalysisExcludedSourceIds")]
    ContentAnalysisExcludedSourceIds,
}
