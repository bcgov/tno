using System;
using System.Collections.Generic;
using System.Linq;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ReportInstanceHistory class, the parts of a report instance needed to exclude or deduplicate content in a later instance.
/// It does not hold the instance's output or its stories' text, which can be megabytes per instance.
/// </summary>
public class ReportInstanceHistory
{
    #region Properties
    /// <summary>
    /// get/set - Report instance identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - When the instance was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - When the instance was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The content in the instance.
    /// </summary>
    public IEnumerable<ReportInstanceHistoryContent> Content { get; set; } = Array.Empty<ReportInstanceHistoryContent>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceHistory object.
    /// </summary>
    public ReportInstanceHistory() { }

    /// <summary>
    /// Creates a new instance of a ReportInstanceHistory object, initializes with specified parameter.
    /// </summary>
    /// <param name="instance"></param>
    public ReportInstanceHistory(ReportInstance instance)
    {
        this.Id = instance.Id;
        this.PublishedOn = instance.PublishedOn;
        this.SentOn = instance.SentOn;
        this.Content = instance.ContentManyToMany.Select(c => new ReportInstanceHistoryContent(c)).ToArray();
    }
    #endregion
}
