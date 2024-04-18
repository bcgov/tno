using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Actions.Managers;
using TNO.Services.Config;

namespace TNO.Services.Actions;

/// <summary>
/// ServiceAction abstract class, provides a generic service that runs a ingestion action, via a data source action manager.
/// When you inherit from this IngestAction, you will need to provide the DataSourceIngestionManager for your action.
/// </summary>
public abstract class IngestAction<TOptions> : ServiceAction<TOptions>, IIngestAction<TOptions>
    where TOptions : IngestServiceOptions
{
    #region Variables
    #endregion

    #region Properties
    private HttpRequestHeaders Headers
    {
        get
        {
            var headers = new HttpRequestMessage().Headers;
            headers.Add("User-Agent", GetType().FullName);
            return headers;
        }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public IngestAction(IApiService api, IOptions<TOptions> options, ILogger<IngestAction<TOptions>> logger) : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified data source.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    public abstract Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Perform the action for the specified data source.
    /// Override the PerformActionAsync(IIngestActionManager manager) method instead of this one.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    public override Task<ServiceActionResult> PerformActionAsync<T>(IServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        return PerformActionAsync((IIngestActionManager)manager, name, data, cancellationToken);
    }

    /// <summary>
    /// Find the content reference for the specified 'source' and 'uid'.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    protected virtual async Task<ContentReferenceModel?> FindContentReferenceAsync(string? source, string? uid)
    {
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException($"Ingest is missing source code.");
        if (String.IsNullOrWhiteSpace(uid)) throw new ArgumentException($"Ingest '{source}' is missing uid.");

        return await this.Api.FindContentReferenceAsync(source, uid);
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    protected virtual async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel? reference, WorkflowStatus status = WorkflowStatus.InProgress)
    {
        if (reference != null)
        {
            if (reference.Status != (int)status) reference.Status = (int)status;
            reference = await this.Api.UpdateContentReferenceAsync(reference);
        }
        return reference;
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// Sends message to Kafka with received source content.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="reference"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    protected virtual async Task<ContentReferenceModel?> ContentReceivedAsync(IIngestActionManager manager, ContentReferenceModel? reference, SourceContent? content)
    {
        if (reference != null &&
            reference.Status != (int)WorkflowStatus.Received &&
            reference.Status != (int)WorkflowStatus.Imported)
        {
            reference = await this.UpdateContentReferenceAsync(reference, WorkflowStatus.Received);
            if (reference != null && manager.Ingest.PostToKafka() && content != null)
            {
                _ = await Api.SendMessageAsync(manager.Ingest.Topic, content) ?? throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
            }
        }
        return reference;
    }

    /// <summary>
    /// Get the TimeZoneInfo for the specified ingest configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected TimeZoneInfo GetTimeZone(IngestModel ingest)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(IngestActionManager<TOptions>.GetTimeZone(ingest, this.Options.TimeZone));
    }

    /// <summary>
    /// Converts the specified date to the ingest timezone.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime ToTimeZone(DateTime date, IngestModel ingest)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(IngestActionManager<TOptions>.GetTimeZone(ingest, this.Options.TimeZone));
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), tz);
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetDateTimeForTimeZone(IngestModel ingest)
    {
        return DateTime.Now.ToTimeZone(IngestActionManager<TOptions>.GetTimeZone(ingest, this.Options.TimeZone));
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetDateTimeForTimeZone(IngestModel ingest, DateTime date)
    {
        return date.ToTimeZone(IngestActionManager<TOptions>.GetTimeZone(ingest, this.Options.TimeZone));
    }

    /// <summary>
    /// Create a unique hash for the specified 'source', 'headline', and 'publishedOn'
    /// </summary>
    /// <param name="source"></param>
    /// <param name="headline"></param>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    protected string GetContentHash(string source, string headline, DateTime? publishedOn)
    {
        var date = publishedOn.HasValue ? $"{publishedOn:yyyy-MM-dd-hh-mm-ss}" : "";
        string hashInput = $"{source}:{headline}:{date}";
        var inputBytes = Encoding.UTF8.GetBytes(hashInput);
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }
    #endregion
}
