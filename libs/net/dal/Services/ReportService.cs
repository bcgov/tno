using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Variables
    private readonly IElasticClient _client;
    private readonly ElasticOptions _elasticOptions;
    #endregion

    #region Constructors
    public ReportService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IElasticClient client,
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
    public async Task<IEnumerable<Content>> FindContentWithElasticsearchAsync(Report report)
    {
        var response = await _client.SearchAsync<Content>(s =>
        {
            var result = s
                .Pretty()
                .Index(_elasticOptions.PublishedIndex) // TODO: Switch to unpublished if the report settings require it.
                .Query(q => q.Raw(report.Filter.ToJson()));

            return result;
        });

        var items = response.IsValid ?
            response.Documents :
            throw new Exception($"Invalid Elasticsearch response: {response.ServerError?.Error?.Reason}");

        return items ?? Array.Empty<Content>();
    }
    #endregion
}
