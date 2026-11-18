namespace TNO.API.Areas.Services.Models.History;

/// <summary>
/// HistoryPurgeModel class, the number of rows a history purge deleted (or would delete, for a
/// dry run), per table. Rows removed by a cascade are counted against their own table.
/// </summary>
public class HistoryPurgeModel
{
    #region Properties
    /// <summary>
    /// get/set - Whether the purge was a dry run that deleted nothing.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// get/set - The retention in days that was applied. Zero means the purge is disabled.
    /// </summary>
    public int RetentionDays { get; set; }

    /// <summary>
    /// get/set - Rows per table, keyed by table name.
    /// </summary>
    public Dictionary<string, long> Tables { get; set; } = new();
    #endregion

    #region Methods
    /// <summary>
    /// Add the specified count to the table total.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="count"></param>
    public void Add(string table, long count)
    {
        this.Tables[table] = this.Tables.TryGetValue(table, out var total) ? total + count : count;
    }
    #endregion
}
