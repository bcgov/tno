namespace TNO.Kafka.Models;

/// <summary>
/// AnalysisRequestModel class, wakes Content-Analysis workers when analysis work is scheduled. The
/// job table is the source of truth; a lost message only delays work until the workers next poll.
/// </summary>
public class AnalysisRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - When the earliest scheduled job becomes due.
    /// </summary>
    public DateTime DueOn { get; set; }

    /// <summary>
    /// get/set - The number of jobs scheduled.
    /// </summary>
    public int Count { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisRequestModel.
    /// </summary>
    public AnalysisRequestModel() { }

    /// <summary>
    /// Creates a new instance of an AnalysisRequestModel, initializes with specified parameters.
    /// </summary>
    /// <param name="dueOn"></param>
    /// <param name="count"></param>
    public AnalysisRequestModel(DateTime dueOn, int count)
    {
        this.DueOn = dueOn;
        this.Count = count;
    }
    #endregion
}
