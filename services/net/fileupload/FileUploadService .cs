using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Services;
using TNO.Services.Managers;
using TNO.Services.FileUpload.Config;
using Microsoft.Extensions.DependencyInjection;

using TNO.Services.Runners;

namespace TNO.Services.FileUpload;

/// <summary>
/// FileUploadService class, provides a service to upload files.
/// </summary>
public class FileUploadService : BaseService
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileUploadService object, initializes with specified parameters.
    /// </summary>
    public FileUploadService(string[] args) : base(args)
    {
    }
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.Configure<FileUploadOptions>(this.Configuration.GetSection("Service"))
                .AddTransient<IServiceManager, FileUploadManager>();
        return services;
    }

    #endregion
}