namespace TNO.Entities;

public interface IReadonlyFileReference : IReadonlyAuditColumns
{
    #region Properties
    long Id { get; }

    long ContentId { get; }

    Content? Content { get; }

    string ContentType { get; }

    string FileName { get; }

    long Size { get; }

    long RunningTime { get; }

    bool IsUploaded { get; }
    #endregion
}
