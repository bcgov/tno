using System.Collections.Generic;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ReportContentMutation class, captures the result of appending content to a report instance.
/// </summary>
public class ReportContentMutation
{
    /// <summary>
    /// Creates a new instance of a ReportContentMutation object.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instance"></param>
    /// <param name="added"></param>
    /// <param name="instanceCreated"></param>
    public ReportContentMutation(int reportId, ReportInstance instance, IEnumerable<ReportInstanceContent> added, bool instanceCreated)
    {
        ReportId = reportId;
        Instance = instance;
        Added = added;
        InstanceCreated = instanceCreated;
    }

    /// <summary>
    /// get - Report identifier.
    /// </summary>
    public int ReportId { get; }

    /// <summary>
    /// get - The instance that received the appended content.
    /// </summary>
    public ReportInstance Instance { get; }

    /// <summary>
    /// get - The collection of content rows that were appended.
    /// </summary>
    public IEnumerable<ReportInstanceContent> Added { get; }

    /// <summary>
    /// get - Indicates whether a brand new instance was generated to receive the append.
    /// </summary>
    public bool InstanceCreated { get; }
}
