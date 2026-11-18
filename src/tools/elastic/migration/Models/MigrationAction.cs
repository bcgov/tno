namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationAction enum, provides a way to define what action to perform during a migration step.
/// </summary>
public enum MigrationAction
{
    /// <summary>
    /// When no action is defined it will throw an error.
    /// </summary>
    NotDefined,
    /// <summary>
    /// Create a new index.
    /// </summary>
    CreateIndex,
    /// <summary>
    /// Deletes an existing index.
    /// </summary>
    DeleteIndex,
    /// <summary>
    /// Updates an existing index mapping.
    /// </summary>
    UpdateMapping,
    /// <summary>
    /// Reindexes an index.
    /// </summary>
    Reindex,
    /// <summary>
    /// Create a new alias.
    /// </summary>
    CreateAlias,
    /// <summary>
    /// Updates an existing alias.
    /// </summary>
    UpdateAlias,
    /// <summary>
    /// Deletes an existing alias.
    /// </summary>
    DeleteAlias,
    /// <summary>
    /// Create a pipeline.
    /// </summary>
    CreatePipeline,
    /// <summary>
    /// Create a Update index using query.
    /// </summary>
    UpdateByQuery,
}
