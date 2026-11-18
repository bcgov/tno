using System;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ReportInstanceHistoryContent class, a story in a report instance with only what duplicate checks compare.
/// </summary>
public class ReportInstanceHistoryContent
{
    #region Properties
    /// <summary>
    /// get/set - Content identifier.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The report section the content is in.
    /// </summary>
    public string SectionName { get; set; } = "";

    /// <summary>
    /// get/set - The content headline.
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// get/set - When the content was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - When the content was posted.
    /// </summary>
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - When the content was last updated.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - When the content was created.
    /// </summary>
    public DateTime? CreatedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceHistoryContent object.
    /// </summary>
    public ReportInstanceHistoryContent() { }

    /// <summary>
    /// Creates a new instance of a ReportInstanceHistoryContent object, initializes with specified parameter.
    /// </summary>
    /// <param name="content"></param>
    public ReportInstanceHistoryContent(ReportInstanceContent content)
    {
        this.ContentId = content.ContentId;
        this.SectionName = content.SectionName;
        this.Headline = content.Content?.Headline;
        this.PublishedOn = content.Content?.PublishedOn;
        this.PostedOn = content.Content?.PostedOn;
        this.UpdatedOn = content.Content?.UpdatedOn;
        this.CreatedOn = content.Content?.CreatedOn;
    }
    #endregion
}
