using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Services.Managers;
using TNO.API.Areas.Services.Models.Content;
using System.Net.Http.Json;
using TNO.Services.FileUpload.Config;
namespace TNO.Services.FileUpload;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using TNO.Core.Http;
public class FileUploadManager : ServiceManager<FileUploadOptions>
{
    #region Variables
    #endregion

    #region Constructors
    public FileUploadManager(
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<FileUploadOptions> options,
        ILogger<FileUploadManager> logger)
        : base(api, chesService, chesOptions, options, logger)
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