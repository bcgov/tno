using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Report class, provides a DB model to manage different types of reports.
/// </summary>
[Cache("report", "lookups")]
[Table("report")]
public class Report : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of report.
    /// </summary>
    [Column("report_type")]
    public ReportType ReportType { get; set; }

    /// <summary>
    /// get/set - The default filter for this report.
    /// </summary>
    [Column("filter")]
    public JsonDocument Filter { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - Collection of report instances.
    /// </summary>
    public virtual List<ReportInstance> Instances { get; } = new List<ReportInstance>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    protected Report() : base() { }

    /// <summary>
    /// Creates a new instance of a Report object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    public Report(string name) : base(name) { }

    /// <summary>
    /// Creates a new instance of a Report object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public Report(int id, string name) : base(id, name) { }
    #endregion
}
