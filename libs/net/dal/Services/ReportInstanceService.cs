using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportInstanceService : BaseService<ReportInstance, long>, IReportInstanceService
{
    #region Properties
    #endregion

    #region Constructors
    public ReportInstanceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ReportInstanceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find the report instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override ReportInstance? FindById(long id)
    {
        return this.Context.ReportInstances
            .Include(ri => ri.Owner)
            .Include(ri => ri.Report).ThenInclude(r => r!.SubscribersManyToMany).ThenInclude(sm2m => sm2m.User)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Source)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Product)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Series)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Contributor)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(c => c.Action)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.TopicsManyToMany).ThenInclude(c => c.Topic)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Labels)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.Tags)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.TimeTrackings)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.FileReferences)
            .Include(ri => ri.ContentManyToMany).ThenInclude(cm2m => cm2m.Content).ThenInclude(c => c!.TonePools)
            .FirstOrDefault(ri => ri.Id == id);
    }

    /// <summary>
    /// Find all report instances for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public IEnumerable<ReportInstance> FindInstancesForReportId(int reportId, int? ownerId)
    {
        var query = this.Context.ReportInstances
            .AsNoTracking()
            .Include(ri => ri.Owner)
            .Where(ri => ri.ReportId == reportId);

        if (ownerId.HasValue)
            query = query.Where(ri => ri.OwnerId == ownerId);

        query = query.OrderByDescending(ri => ri.Id);

        return query.ToArray();
    }

    /// <summary>
    /// Add the new report instance to the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ReportInstance Add(ReportInstance entity)
    {
        // Elasticsearch can contain content that does not exist in the database regrettably.
        // While this should not occur, it's possible.
        // Extract any content that does not exist in the database.
        var contentIds = entity.ContentManyToMany.Select(c => c.ContentId);
        var existingContentIds = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => c.Id);
        var reportInstanceContent = entity.ContentManyToMany.Where(c => existingContentIds.Contains(c.ContentId)).ToArray();
        reportInstanceContent.ForEach(ric => this.Context.ReportInstanceContents.Add(ric));
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ReportInstance Update(ReportInstance entity)
    {
        // Elasticsearch can contain content that does not exist in the database regrettably.
        // While this should not occur, it's possible.
        // Extract any content that does not exist in the database.
        var contentIds = entity.ContentManyToMany.Select(c => c.ContentId);
        var existingContentIds = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => c.Id);

        // Fetch all content currently belonging to this report instance.
        var original = this.Context.ReportInstances.FirstOrDefault(ri => ri.Id == entity.Id) ?? throw new InvalidOperationException("Report instance does not exist");
        var originalContent = this.Context.ReportInstanceContents.Where(ric => ric.InstanceId == entity.Id).ToArray();

        // Delete removed content and add new content.
        originalContent.Except(entity.ContentManyToMany).ForEach(c =>
        {
            this.Context.Entry(c).State = EntityState.Deleted;
        });
        entity.ContentManyToMany.Where(c => existingContentIds.Contains(c.ContentId)).ForEach(c =>
        {
            var current = originalContent.FirstOrDefault(o => o.ContentId == c.ContentId && o.SectionName == c.SectionName);
            if (current == null)
                original.ContentManyToMany.Add(c);
            else if (current.SortOrder != c.SortOrder)
            {
                current.SortOrder = c.SortOrder;
                this.Context.Entry(current).State = EntityState.Modified;
            }
        });

        original.OwnerId = entity.OwnerId;
        original.PublishedOn = entity.PublishedOn;
        original.ReportId = entity.ReportId;
        original.Response = entity.Response;
        original.Version = entity.Version;

        return base.Update(original);
    }
    #endregion
}
