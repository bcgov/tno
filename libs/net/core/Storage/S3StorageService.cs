using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.S3;
using TNO.Core.Storage.Configuration;

namespace TNO.Core.Storage;

public class S3StorageService : IS3StorageService
{
    #region Properties
    private readonly S3Options _s3Options;
    protected ILogger<S3StorageService> _logger { get; }
    private static AmazonS3Client? _client;
    #endregion

    #region Constructors
    public S3StorageService(
        IOptions<S3Options> s3Options,
        ILogger<S3StorageService> logger)
    {
        _s3Options = s3Options.Value;
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = CreateS3Client();
    }
    #endregion

    #region Methods

    private AmazonS3Client? CreateS3Client()
    {
        if (!_s3Options.IsS3Enabled)
        {
            return null;
        }

        try
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(_s3Options.AccessKey, _s3Options.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = _s3Options.ServiceUrl,
                    ForcePathStyle = true
                });
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UploadToS3Async(string s3Key, Stream fileStream)
    {
        if (fileStream == null)
        {
            _logger.LogError("File stream is null for S3 key: {S3Key}", s3Key);
            return false;
        }
        if (!_s3Options.IsS3Enabled || await TestS3NetworkConnectionAsync() == false)
        {
            return false;
        }

        if (_client == null)
        {
            return false;
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _s3Options.BucketName,
            Key = s3Key,
            InputStream = fileStream,
        };

        try
        {
            var response = await _client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("File uploaded to S3 successfully: {S3Key}", s3Key);
                return true;
            }
            else
            {
                _logger.LogError("Failed to upload file to S3: {S3Key}", s3Key);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading file to S3: {S3Key}", s3Key);
            return false;
        }
    }
    public async Task<Stream?> DownloadFromS3Async(string s3Key)
    {
        if (!_s3Options.IsS3Enabled || await TestS3NetworkConnectionAsync() == false)
        {
            return null;
        }

        if (_client == null)
        {
            return null;
        }

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _s3Options.BucketName,
                Key = s3Key
            };

            var response = await _client.GetObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            else
            {
                _logger.LogError("Failed to download file from S3: {S3Key}", s3Key);
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// Test the network connection to S3.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TestS3NetworkConnectionAsync()
    {
        if (!_s3Options.IsS3Enabled)
        {
            return false;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var request = new HttpRequestMessage(HttpMethod.Get, _s3Options.ServiceUrl);
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var content = await response.Content.ReadAsStringAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Network connection failed: {ex}");
            return false;
        }
    }

}
#endregion