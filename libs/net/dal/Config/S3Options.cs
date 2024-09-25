using Amazon.Runtime;
using Amazon.S3;
using System.ComponentModel.DataAnnotations;

namespace TNO.DAL.Config;

/// <summary>
/// S3 config
/// </summary>
public class S3Options
{
    public string BucketName => Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "";
    public string ServiceUrl => Environment.GetEnvironmentVariable("S3_SERVICE_URL") ?? "";
    private string AccessKey => Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? "";
    private string SecretKey => Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? "";

    public AWSCredentials GetAWSCredentials()
    {
        return new BasicAWSCredentials(AccessKey, SecretKey);
    }

    public AmazonS3Config GetS3ClientConfig()
    {
        return new AmazonS3Config
        {
            ServiceURL = ServiceUrl,
            ForcePathStyle = true
        };
    }

    // test network connection to s3
    public async Task<bool> TestNetworkConnectionAsync()
    {
        if (!IsS3Enabled)
        {
            return false;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var request = new HttpRequestMessage(HttpMethod.Get, ServiceUrl);
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var content = await response.Content.ReadAsStringAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"network connection failed: {ex.Message}");
            return false;
        }
    }

    // check if s3 is enabled
    public bool IsS3Enabled => !string.IsNullOrEmpty(BucketName) &&
                               !string.IsNullOrEmpty(ServiceUrl) &&
                               !string.IsNullOrEmpty(AccessKey) &&
                               !string.IsNullOrEmpty(SecretKey);

    private AmazonS3Client? _s3Client;
    public AmazonS3Client? S3Client
    {
        get
        {
            if (!IsS3Enabled)
            {
                return null;
            }

            try
            {
                return _s3Client ??= new AmazonS3Client(GetAWSCredentials(), GetS3ClientConfig());
            }
            catch
            {
                return null;
            }
        }
    }
}