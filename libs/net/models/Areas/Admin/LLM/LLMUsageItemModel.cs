namespace TNO.API.Areas.Admin.Models.LLM;

/// <summary>
/// LLMUsageItemModel class, provides a model for one thing that uses an LLM connection.
/// </summary>
public class LLMUsageItemModel
{
    #region Properties
    /// <summary>
    /// get/set - The kind of item [ReportSection, AutomationProfile, ContentAnalysis, Search].
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// get/set - The primary key of the report or automation profile, when the item has one.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// get/set - The report section, when the item is one.
    /// </summary>
    public int? SectionId { get; set; }

    /// <summary>
    /// get/set - The name of the item.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Where in the item the connection is used.
    /// </summary>
    public string Detail { get; set; } = "";

    /// <summary>
    /// get/set - The username of the report owner.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// get/set - Whether the item is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    #endregion
}
