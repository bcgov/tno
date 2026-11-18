namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationStepName enum, the steps of a migration that can run on their own.
/// </summary>
public enum MigrationStepName
{
    /// <summary>Every step, in order.</summary>
    All,

    /// <summary>Create the new indexes, ready for loading and for a second indexing service.</summary>
    Prepare,

    /// <summary>Copy the existing documents into the new indexes.</summary>
    Copy,

    /// <summary>Compare the new indexes with the database and repair differences.</summary>
    Verify,

    /// <summary>Restore the index settings, move the aliases, and record the migration.</summary>
    Cutover,
}
