using System.Net;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Kafka.Models;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public IngestAction(IApiService api, IOptions<TOptions> options) : base(api, options)
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
    public abstract Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Perform the action for the specified data source.
    /// Override the PerformActionAsync(IIngestServiceActionManager manager) method instead of this one.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    public override Task PerformActionAsync<T>(IServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        return PerformActionAsync((IIngestServiceActionManager)manager, name, data, cancellationToken);
    }

    /// <summary>
    /// Update the content reference with the Kafka position information.
    /// Send AJAX request to api to update content reference.
    /// This content reference has been successfully received by Kafka.
    /// Handles optimistic concurrency issue resulting from a race condition with the Content Service.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, DeliveryResultModel<SourceContent>? result)
    {
        if (result != null)
        {
            reference.Offset = result.Offset;
            reference.Partition = result.Partition;
        }
        if (reference.Status != (int)WorkflowStatus.Imported)
            reference.Status = (int)WorkflowStatus.Received;
        return await UpdateContentReferenceAsync(reference);
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// This content reference has been successfully received by Kafka.
    /// Handles optimistic concurrency issue resulting from a race condition with the Content Service.
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    protected virtual async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference)
    {
        try
        {
            return await this.Api.UpdateContentReferenceAsync(reference);
        }
        catch (HttpClientRequestException ex)
        {
            if (ex.Response != null && ex.Response.Content != null)
            {
                // The content service will often already have imported this content before we can update the content reference.
                // TODO: Not certain if the body of the error will always contain `DbUpdateConcurrencyException`.
                var body = await ex.Response.Content.ReadAsStringAsync();
                if (ex.StatusCode == HttpStatusCode.BadRequest && body?.Contains("DbUpdateConcurrencyException") == true)
                {
                    var current = await this.Api.FindContentReferenceAsync(reference.Source, reference.Uid);
                    if (current != null)
                    {
                        // Do not change the status if it has been imported.
                        if (current.Status == (int)WorkflowStatus.Imported)
                            reference.Status = (int)WorkflowStatus.Imported;
                        reference.Version = current.Version;
                        return await this.Api.UpdateContentReferenceAsync(reference);
                    }
                }
            }

            throw;
        }
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
        return TimeZoneInfo.ConvertTimeToUtc(date, tz);
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
    #endregion
}
