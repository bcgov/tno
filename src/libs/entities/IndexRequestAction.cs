namespace TNO.Entities;

/// <summary>
/// IndexRequestAction enum, what an index request asks the indexer to do (mirrors the Kafka
/// IndexAction).
/// </summary>
public enum IndexRequestAction
{
    /// <summary>Add the content to the unpublished index.</summary>
    Index = 0,
    /// <summary>Add the content to the published index.</summary>
    Publish = 1,
    /// <summary>Remove the content from the published index.</summary>
    Unpublish = 2,
    /// <summary>Delete the content from both indexes.</summary>
    Delete = 3,
}
