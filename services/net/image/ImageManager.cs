using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail;
using TNO.Services.Image.Config;
using TNO.Services.Managers;

namespace TNO.Services.Image;

/// <summary>
/// ImageManager class, provides a way to manage the image service.
/// </summary>
public class ImageManager : IngestManager<ImageIngestActionManager, ImageOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="emailService"></param>
    /// <param name="smtpOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        IngestManagerFactory<ImageIngestActionManager, ImageOptions> factory,
        IOptions<ImageOptions> options,
        ILogger<ImageManager> logger)
        : base(serviceProvider, api, emailService, smtpOptions, factory, options, logger)
    {
    }
    #endregion
}
