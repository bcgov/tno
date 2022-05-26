using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Models.Extensions;
using TNO.Services.Capture.Config;
using TNO.Services.Command;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureAction class, performs the capture ingestion action.
/// Fetch capture feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class CaptureAction : CommandAction<CaptureOptions>
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureAction(IApiService api, IOptions<CaptureOptions> options, ILogger<CaptureAction> logger) : base(api, options, logger)
    {
    }
    #endregion

    #region Methods

    /// <summary>
    /// Generate the command for the service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    protected override string GenerateCommand(IDataSourceIngestManager manager)
    {
        var input = GetUrl(manager.DataSource);
        var path = GetOutputPath(manager.DataSource);
        var fileName = GetFileName(manager.DataSource);
        var format = GetFormat(manager.DataSource);
        var volume = GetVolume(manager.DataSource);
        var otherArgs = GetOtherArgs(manager.DataSource);
        var output = Path.Combine(path, fileName);

        Directory.CreateDirectory(path);

        return $"{this.Options.Command} {input}{volume}{format}{otherArgs} {output}";
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected string GetOutputPath(DataSourceModel dataSource)
    {
        return Path.Combine(this.Options.OutputPath, $"{dataSource.Code}/{GetLocalDateTime(dataSource, DateTime.Now):yyyy-MM-dd}");
    }

    /// <summary>
    /// Get the URL from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetUrl(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("url");
        var options = new UriCreationOptions();
        if (!Uri.TryCreate(value, options, out Uri? uri)) throw new InvalidOperationException("Data source connection 'url' is not a valid format.");
        return $"-i {uri.ToString().Replace(" ", "+")}";
    }

    /// <summary>
    /// Get the file name from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetFileName(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("ffileNamermat");
        var filename = String.IsNullOrWhiteSpace(value) ? "stream.mp3" : value;
        var name = Path.GetFileName(filename);
        var ext = Path.GetExtension(filename);
        return $"{name}-{GetLocalDateTime(dataSource, DateTime.Now):HH-mm-ss}.{ext}";
    }

    /// <summary>
    /// Get the format from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetFormat(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("format");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -f {value}";
    }

    /// <summary>
    /// Get the volume from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetVolume(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("volume");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -filter:a 'volume={value}'";
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetOtherArgs(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("otherArgs");
        return String.IsNullOrWhiteSpace(value) ? "" : $" {value}";
    }
    #endregion
}
