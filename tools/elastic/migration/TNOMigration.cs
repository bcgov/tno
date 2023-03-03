using Microsoft.Extensions.Logging;
using Nest;
using TNO.API.Areas.Services.Models.Content;
using TNO.DAL.Models;
using TNO.DAL.Services;

namespace TNO.Elastic.Migration;

/// <summary>
/// TNOMigration abstract class, provides helper methods for migrating the TNO elasticsearch.
/// </summary>
public abstract class TNOMigration : Migration
{
    #region Variables
    private readonly IContentService _contentService;
    #endregion

    #region  Constructors
    /// <summary>
    /// Creates a new instance of a TNOMigration object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    public TNOMigration(MigrationBuilder builder, IContentService contentService) : base(builder)
    {
        _contentService = contentService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Iterate through database and reindex content that matches the specified 'filter'.
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
                var model = new ContentModel(item);
                var unpublishedIndex = new IndexRequest<ContentModel>(model, $"{builder.MigrationOptions.UnpublishedIndex}_v{this.Version}", model.Id);
                var response = await builder.Client.IndexAsync(unpublishedIndex, cancellationToken);
                if (!response.IsValid) builder.Logger.LogError(response.OriginalException, "Failed to index Content:{id} in Index:{index}.  Error:{error}", model.Id, unpublishedIndex, response.ServerError);

                if (item.Status == Entities.ContentStatus.Published)
                {
                    var publishedIndex = new IndexRequest<ContentModel>(model, $"{builder.MigrationOptions.PublishedIndex}_v{this.Version}", model.Id);
                    response = await builder.Client.IndexAsync(publishedIndex, cancellationToken);
                    if (!response.IsValid) builder.Logger.LogError(response.OriginalException, "Failed to index Content:{id} in Index:{index}.  Error:{error}", model.Id, publishedIndex, response.ServerError);
                }

                builder.Logger.LogDebug("Content reindexed {index} of {total}, ID: {id}", index++, page.Total, model.Id);
            }
            reindex = page.Items.Count == filter.Quantity;
        }
    }
    #endregion
}
