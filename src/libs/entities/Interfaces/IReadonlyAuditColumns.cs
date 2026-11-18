namespace TNO.Entities;

public interface IReadonlyAuditColumns
{
    string CreatedBy { get; }

    DateTime CreatedOn { get; }

    string UpdatedBy { get; }

    DateTime UpdatedOn { get; }

    /// <summary>
    /// get/set - The concurrency version value for the row.
    /// </summary>
    long Version { get; }
}
