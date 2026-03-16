using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail;
using TNO.Services.FileUpload.Config;
using TNO.Services.Managers;

namespace TNO.Services.FileUpload;

public class FileUploadManager : ServiceManager<FileUploadOptions>
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileUploadManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="emailService"></param>
    /// <param name="smtpOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileUploadManager(
        IApiService api,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        IOptions<FileUploadOptions> options,
        ILogger<FileUploadManager> logger)
        : base(api, emailService, smtpOptions, options, logger)
    {
    }
    #endregion

    #region Methods
    public override async Task RunAsync()
    {
        if (this.State.Status != ServiceStatus.Running)
        {
            this.Logger.LogDebug("service is not running: '{Status}'", this.State.Status);
            return;
        }

        try
        {
            this.Logger.LogInformation("start upload files to S3");

            var now = DateTime.UtcNow.Date;
            var daysBeforeStart = this.Options.DaysBeforeStart;
            var daysBeforeEnd = this.Options.DaysBeforeEnd;

            var publishedAfter = now.AddDays(-daysBeforeStart);
            var publishedBefore = now.AddDays(-daysBeforeEnd);
            this.Logger.LogInformation($"Date range: {publishedAfter} to {publishedBefore}");

            var response = await this.Api.UploadFilesToS3Async(
                publishedAfter: publishedAfter,
                publishedBefore: publishedBefore,
                limit: this.Options.Limit
            );

        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException httpEx)
            {
                this.Logger.LogError(ex, "HTTP request exception");
            }
            else
            {
                this.Logger.LogError(ex, "service has an unexpected error. exception type: {ExceptionType}", ex.GetType().Name);
            }
            this.State.RecordFailure();
        }
    }
    #endregion
}
