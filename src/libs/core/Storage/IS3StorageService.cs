namespace TNO.Core.Storage;

public interface IS3StorageService
{
    Task<bool> UploadToS3Async(string s3Key, Stream fileStream);

    Task<Stream?> DownloadFromS3Async(string s3Key);

    Task<bool> TestS3NetworkConnectionAsync();

    Task<double?> GetMediaDurationAsync(string s3Key);
}
