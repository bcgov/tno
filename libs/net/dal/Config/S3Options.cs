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

    // test network connection to s3
    public static async Task<bool> TestNetworkConnectionAsync()
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




    // Check if S3 is enabled, which is determined by the presence of the S3_BUCKET_NAME, S3_SERVICE_URL, S3_ACCESS_KEY, and S3_SECRET_KEY environment variables.
    // If any of these are missing, S3 is not enabled. 
    public static bool IsS3Enabled
    {
        get
        {
            return !string.IsNullOrEmpty(BucketName) &&
                   !string.IsNullOrEmpty(ServiceUrl) &&
                   !string.IsNullOrEmpty(AccessKey) &&
                   !string.IsNullOrEmpty(SecretKey);
        }
    }


    private static AmazonS3Client? _s3Client;
    // If S3 is enabled, create a new AmazonS3Client with the AWS credentials and S3 client configuration.
    public static AmazonS3Client? S3Client
    {
        get
        {
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

    public static string BucketName => Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "";
    public static string ServiceUrl => Environment.GetEnvironmentVariable("S3_SERVICE_URL") ?? "";
    private static string AccessKey => Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? "";
    private static string SecretKey => Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? "";
}