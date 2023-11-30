using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
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
            .Include(ri => ri.Report).ThenInclude(r => r!.Template)
            .Include(ri => ri.Report).ThenInclude(r => r!.SubscribersManyToMany).ThenInclude(sm2m => sm2m.User)
            .Include(ri => ri.Report).ThenInclude(r => r!.Sections).ThenInclude(s => s.Filter)
            .Include(ri => ri.Report).ThenInclude(r => r!.Sections).ThenInclude(s => s.ChartTemplatesManyToMany).ThenInclude(ct => ct.ChartTemplate)
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
    /// Get all the content items for the specified instance.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<ReportInstanceContent> GetContentForInstance(long id)
    {
        return this.Context.ReportInstanceContents
            .AsNoTracking()
            .Include(cm2m => cm2m.Content)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.Source)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.MediaType)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.Series)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.Contributor)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.ActionsManyToMany).ThenInclude(c => c.Action)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.TopicsManyToMany).ThenInclude(c => c.Topic)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.Labels)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.TagsManyToMany)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.TimeTrackings)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.FileReferences)
            .Include(cm2m => cm2m.Content).ThenInclude(c => c!.TonePoolsManyToMany).ThenInclude(t => t.TonePool)
            .Where(ric => ric.InstanceId == id)
            .ToArray();
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
        var contentIds = entity.ContentManyToMany.Select(c => c.ContentId).ToArray();
        var existingContentIds = this.Context.Contents.Where(c => contentIds.Contains(c.Id)).Select(c => c.Id).ToArray();
        var reportInstanceContent = entity.ContentManyToMany.Where(c => existingContentIds.Contains(c.ContentId)).ToArray();
        reportInstanceContent.ForEach(ric =>
        {
            ric.Instance = entity;
            ric.InstanceId = entity.Id;
            this.Context.ReportInstanceContents.Add(ric);
        });
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ReportInstance Update(ReportInstance entity)
    {
        // Fetch all content currently belonging to this report instance.
        var original = this.Context.ReportInstances.FirstOrDefault(ri => ri.Id == entity.Id) ?? throw new InvalidOperationException("Report instance does not exist");
        var originalInstanceContent = this.Context.ReportInstanceContents
            .AsNoTracking()
            .Include(ic => ic.Content)
            .Where(ric => ric.InstanceId == entity.Id).ToArray();

        // Delete removed content and add new content.
        originalInstanceContent.Except(entity.ContentManyToMany).ForEach(ric =>
        {
            this.Context.Entry(ric).State = EntityState.Deleted;
            // Content that is private will only exist on a single report.
            // If it's removed from the report we can remove it from the database.
            if (ric.Content?.IsPrivate == true) this.Context.Entry(ric.Content).State = EntityState.Deleted;
        });
        entity.ContentManyToMany.ForEach(ric =>
        {
            var existingInstanceContent = originalInstanceContent.FirstOrDefault(o => o.ContentId == ric.ContentId && o.SectionName == ric.SectionName);
            if (existingInstanceContent == null)
            {
                // If the content is new too, upload it to the database.
                if (ric.Content?.Id == 0)
                {
                    ric.Content.IsPrivate = true;
                    ric.Content.GuaranteeUid();
                    ric.Content.AddToContext(this.Context);
                    this.Context.Add(ric.Content);
                }
                else
                {
                    // Make certain the content exists in the database.
                    if (!this.Context.Contents.Any(c => c.Id == ric.ContentId)) throw new InvalidOperationException($"Content '{ric.ContentId}' does not exist.");

                    // Do not allow updating content not owned by the user.
                    if (ric.Content != null)
                    {
                        // TODO: Small security issue as the JSON could lie about this data.
                        if (ric.Content.IsPrivate)
                            this.Context.Entry(ric.Content).State = EntityState.Modified;
                        else
                            this.Context.Entry(ric.Content).State = EntityState.Unchanged;
                    }
                }

                // Add new content to the report.
                ric.Instance = null;
                ric.InstanceId = entity.Id;
                this.Context.ReportInstanceContents.Add(ric);
            }
            else
            {
                if (ric.Content != null &&
                    existingInstanceContent.Content != null &&
                    entity.OwnerId.HasValue)
                {
                    // TODO: Small security issue as the JSON could lie about this data.
                    if (ric.Content.IsPrivate)
                    {
                        if (ric.Content.GuaranteeUid() && existingInstanceContent.Content.Uid != ric.Content.Uid) existingInstanceContent.Content.Uid = ric.Content.Uid;
                        this.Context.UpdateContext(existingInstanceContent.Content, ric.Content);
                        this.Context.Update(existingInstanceContent.Content);
                    }
                    else if (ric.Content.Versions.ContainsKey(entity.OwnerId.Value))
                    {
                        // Update the content versions if they have been provided.
                        if (existingInstanceContent.Content.Versions.ContainsKey(entity.OwnerId.Value))
                            existingInstanceContent.Content.Versions[entity.OwnerId.Value] = ric.Content.Versions[entity.OwnerId.Value];
                        else
                            existingInstanceContent.Content.Versions.Add(entity.OwnerId.Value, ric.Content.Versions[entity.OwnerId.Value]);
                        this.Context.Update(existingInstanceContent.Content);
                    }
                }
                if (existingInstanceContent.SortOrder != ric.SortOrder)
                {
                    existingInstanceContent.SortOrder = ric.SortOrder;
                    this.Context.Update(existingInstanceContent);
                }
            }
        });

        original.Status = entity.Status;
        original.OwnerId = entity.OwnerId;
        original.PublishedOn = entity.PublishedOn;
        original.SentOn = entity.SentOn;
        original.ReportId = entity.ReportId;
        original.Subject = entity.Subject;
        original.Body = entity.Body;
        original.Response = entity.Response;
        original.Version = entity.Version;
        this.Context.ResetVersion(original);

        return base.Update(original);
    }
    #endregion
}
