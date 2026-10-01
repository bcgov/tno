using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AnalysisTopic class, provides a DB model for the analysis topic registry: extracted topics that
/// match no staff topic, with stable IDs and deterministic aliases (no fuzzy-only merging). Report
/// synthesis groups by the matched staff topic when there is one, otherwise by this label.
/// </summary>
[Table("analysis_topic")]
public class AnalysisTopic : AuditColumns
{
    #region Properties
    /// <summary>get/set - Primary key (stable).</summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>get/set - The label shown in reports.</summary>
    [Column("label")]
    public string Label { get; set; } = "";

    /// <summary>get/set - The deterministic key labels are matched on (lower case, punctuation and extra spaces removed).</summary>
    [Column("key")]
    public string Key { get; set; } = "";

    /// <summary>get/set - Other keys that resolve to this entry.</summary>
    [Column("aliases")]
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>get/set - Foreign key to the staff topic the entry matches, when one does.</summary>
    [Column("topic_id")]
    public int? TopicId { get; set; }

    /// <summary>get/set - The staff topic.</summary>
    public virtual Topic? Topic { get; set; }

    /// <summary>get/set - The registry version the entry was added or changed in.</summary>
    [Column("registry_version")]
    public int RegistryVersion { get; set; } = 1;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisTopic object.
    /// </summary>
    protected AnalysisTopic() { }

    /// <summary>
    /// Creates a new instance of an AnalysisTopic object, initializes with specified parameters.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="key"></param>
    public AnalysisTopic(string label, string key)
    {
        this.Label = label;
        this.Key = key;
    }
    #endregion
}
