using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.DAL.Config;
using TNO.DAL.Elasticsearch;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Variables
    private readonly ITnoElasticClient _client;
    private readonly ElasticOptions _elasticOptions;
    #endregion

    #region Constructors
    public ReportService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITnoElasticClient client,
        IOptions<ElasticOptions> elasticOptions,
        IServiceProvider serviceProvider,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _client = client;
        _elasticOptions = elasticOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the reports.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Report> FindAll()
    {
        return this.Context.Reports
            .AsNoTracking()
            .Include(n => n.SubscribersManyToMany).ThenInclude(s => s.User)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    /// <summary>
    /// Find the report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Report? FindById(int id)
    {
        return this.Context.Reports
            .Include(n => n.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(n => n.Id == id);
    }

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IEnumerable<API.Areas.Services.Models.Content.ContentModel>> FindContentWithElasticsearchAsync(Report report)
    {
        // var response = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(s =>
        // {
        //     var json = report.Filter.ToJson();
        //     var result = s
        //         .Pretty()
        //         .Index(_elasticOptions.PublishedIndex) // TODO: Switch to unpublished if the report settings require it.
        //         .Query(q => q.Raw(json == "{}" ? "" : json));

        //     var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(report.Settings) ?? new Dictionary<string, object>();
        //     var page = settings.GetDictionaryJsonValue<int>("page");
        //     var quantity = settings.GetDictionaryJsonValue<int>("quantity");
        //     if (quantity > 0)
        //     {
        //         result.From((page - 1) * quantity)
        //             .Size(quantity);
        //     }

        //     return result;
        // });

        // var items = response.IsValid ?
        //     response.Documents :
        //     throw new Exception($"Invalid Elasticsearch response: {response.ServerError?.Error?.Reason}");

        // return items;

        var r = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(_elasticOptions.UnpublishedIndex, report.Filter);
        var docs = r?.Hits.Hits.Select(h => h.Source) ?? Array.Empty<API.Areas.Services.Models.Content.ContentModel>();

        // TODO: handle paging results.
        return docs;
    }
    #endregion
}
