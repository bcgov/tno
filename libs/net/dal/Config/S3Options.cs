using Amazon.Runtime;
using Amazon.S3;

namespace TNO.DAL.Config;
/// <summary>
/// S3 config
/// </summary>
public static class S3Options
{

    public static AWSCredentials GetAWSCredentials()
    {
        return new BasicAWSCredentials(AccessKey, SecretKey);
    }

    public static AmazonS3Config GetS3ClientConfig()
    {
        return new AmazonS3Config
        {
            ServiceURL = ServiceUrl,
            ForcePathStyle = true
        };
    }

    private static AmazonS3Client? _s3Client;
    public static AmazonS3Client S3Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client(GetAWSCredentials(), GetS3ClientConfig());
            }
            return _s3Client;
        }
    }

    public static string BucketName => Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? throw new InvalidOperationException("S3_BUCKET_NAME environment variable is not set");
    public static string ServiceUrl => Environment.GetEnvironmentVariable("S3_SERVICE_URL") ?? throw new InvalidOperationException("S3_SERVICE_URL environment variable is not set");
    private static string AccessKey => Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? throw new InvalidOperationException("S3_ACCESS_KEY environment variable is not set");
    private static string SecretKey => Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? throw new InvalidOperationException("S3_SECRET_KEY environment variable is not set");
}