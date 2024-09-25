namespace TNO.DAL.Config;

/// <summary>
/// S3 config
/// </summary>
public class S3Options
{
    public string BucketName { get; set; } = "";
    public string ServiceUrl { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    // check if s3 is enabled
    public bool IsS3Enabled => !string.IsNullOrEmpty(BucketName) &&
                               !string.IsNullOrEmpty(ServiceUrl) &&
                               !string.IsNullOrEmpty(AccessKey) &&
                               !string.IsNullOrEmpty(SecretKey);

}