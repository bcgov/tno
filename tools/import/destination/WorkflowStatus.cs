namespace TNO.Tools.Import.Destination;

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
    /// Content has been received from data source and is in progress of being
    /// ingested. It has not yet been added to TNO.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Content has been received by TNO, but is not searchable.
    /// </summary>
    Received = 1,

    /// <summary>
    /// Content has been received and transcribed in TNO but is not yet searchable.
    /// </summary>
    Transcribed = 2,

    /// <summary>
    /// Content has been received and Natural Language Processed in TNO but is not yet searchable.
    /// </summary>
    NLP = 3,

    /// <summary>
    /// Content has successfully been added to TNO, and is searchable.
    /// </summary>
    Success = 4,

    /// <summary>
    /// Content has been published.
    /// </summary>
    Published = 5,

    /// <summary>
    /// Content has been unpublished.
    /// </summary>
    Unpublished = 6
}