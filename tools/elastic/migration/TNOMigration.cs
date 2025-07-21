using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.DAL.Services;
using TNO.Models.Filters;

namespace TNO.Elastic.Migration;

/// <summary>
/// TNOMigration abstract class, provides helper methods for migrating the TNO elasticsearch.
/// </summary>
public abstract class TNOMigration : Migration
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region  Constructors
    /// <summary>
    /// Creates a new instance of a TNOMigration object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="serializerOptions"></param>
    public TNOMigration(MigrationBuilder builder, IContentService contentService, IOptions<JsonSerializerOptions> serializerOptions) : base(builder)
    {
        _contentService = contentService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Iterate through database and reindex content that matches the specified 'filter'.
    /// TODO: NEST does not use System.Text.Json, and as such most of the serialization cannot be controlled through configuration.
    /// Additionally, it uses a version of Newtonsoft that has a few challenges if you want to write your own serializer.
    /// For some reason we can't get version 8 of NEST which resolves these issues.
    /// This means that for now we cannot index documents with this method as it will corrupt the data because it does not have the correct serialization.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="filter"></param>
    /// <param name="cancellationToken"></param>
    protected async Task ReindexAsync(MigrationBuilder builder, ContentFilter filter, CancellationToken cancellationToken = default)
    {
        builder.Logger.LogInformation("Reindexing all content for filter");
        var reindex = true;
        var index = 1;
        filter.Page = 0;
        filter.Quantity = 100;
        filter.Sort = new[] { "Id" };

        while (reindex)
        {
            filter.Page += 1;
            var page = _contentService.FindWithDatabase(filter, true);

            foreach (var item in page.Items)
            {
                // Try the same item until it is successful.
                var indexed = false;
                while (!indexed)
                {
                    try
                    {
                        var model = new ContentModel(item, _serializerOptions);
                        var unpublishedRequest = new IndexRequest<ContentModel>(model, $"{builder.MigrationOptions.ContentIndex}_v{this.Version}", model.Id);
                        var response = await builder.IndexingClient.IndexAsync(unpublishedRequest, cancellationToken);
                        if (!response.IsValidResponse)
                        {
                            if (response.TryGetOriginalException(out Exception? ex))
                                throw ex!;
                            throw new Exception($"Failed to index. Error {response.ElasticsearchServerError?.Error.Reason}");
                        }

                        if (item.Status == Entities.ContentStatus.Published)
                        {
                            var publishedRequest = new IndexRequest<ContentModel>(model, $"{builder.MigrationOptions.PublishedIndex}_v{this.Version}", model.Id);
                            response = await builder.IndexingClient.IndexAsync(publishedRequest, cancellationToken);
                            if (!response.IsValidResponse)
                            {
                                if (response.TryGetOriginalException(out Exception? ex))
                                    throw ex!;
                                throw new Exception($"Failed to index. Error {response.ElasticsearchServerError?.Error.Reason}");
                            }
                        }

                        builder.Logger.LogDebug("Content reindexed {index} of {total}, ID: {id}", index++, page.Total, model.Id);
                        this.Failures = 0;
                        indexed = true;
                    }
                    catch (TransportException ex)
                    {
                        builder.Logger.LogError(ex, "Failed to index content ID: {id}", item.Id);
                        // Ignore 502 gateway error, for some reason our cluster has intermittent issues.
                        if (ex.ApiCallDetails.HttpStatusCode != 502)
                        {
                            if (++this.Failures >= builder.MigrationOptions.ReindexFailureLimit) throw;
                        }
                    }
                }
            }
            Thread.Sleep(builder.MigrationOptions.ReindexDelay);
            reindex = page.Items.Count == filter.Quantity;
        }
    }
    #endregion
}
