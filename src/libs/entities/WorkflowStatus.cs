namespace TNO.Entities;

/// <summary>
///  Provides workflow content status to determine the stage the content is in.
///  A workflow status only represents a moment of time.
///  It informs a user what process is being performed.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// Content has failed to be added to TNO.
    /// </summary>
    Failed = -1,

    /// <summary>
    /// Content has been discovered and is currently being ingested.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Content has been ingested, files have been copied, and message posted to Kafka.
    /// </summary>
    Received = 1,

    /// <summary>
    /// Content has been imported into the TNO database, and files have been copied into location API has access to.
    /// </summary>
    Imported = 2
}
