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
            .AsSplitQuery()
            .FirstOrDefault(ri => ri.Id == id);
    }

    /// <summary>
    /// Find all report instances for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public IEnumerable<ReportInstance> FindInstancesForReportId(int reportId, int? ownerId, int skip = 0, int take = 10)
    {
        var query = this.Context.ReportInstances
            .AsNoTracking()
            .Include(ri => ri.Owner)
            .Where(ri => ri.ReportId == reportId);

        if (ownerId.HasValue)
            query = query.Where(ri => ri.OwnerId == ownerId);

        query = query.OrderByDescending(ri => ri.Id).Skip(skip).Take(take);

        return query.ToArray();
    }

    /// <summary>
    /// Find report instances for the specified 'reportId' and 'date'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public ReportInstance? FindInstanceForReportIdAndDate(long reportId, DateTime date)
    {
        DateTime morning = new(date.Year, date.Month, date.Day, 0, 0, 0);
        DateTime midnight = morning.AddHours(24);
        return this.Context.ReportInstances
            .AsNoTracking()
            .Include(ri => ri.Report)
            .Include(ri => ri.Owner)
            .Where(ri => ri.ReportId == reportId && ri.PublishedOn >= morning.ToUniversalTime() && ri.PublishedOn <= midnight.ToUniversalTime())
            .FirstOrDefault();
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
            .OrderBy(ric => ric.SortOrder)
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
            if (ric.ContentId == 0)
                this.Context.Add(ric);
            else
                this.Context.Entry(ric).State = EntityState.Added;
        });
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    public ReportInstance Update(ReportInstance entity, bool updateChildren = false)
    {
        if (updateChildren)
        {
            // Fetch all content currently belonging to this report instance.
            var original = this.Context.ReportInstances
                .Include(ri => ri.ContentManyToMany)
                .ThenInclude(ric => ric.Content)
                .FirstOrDefault(ri => ri.Id == entity.Id) ?? throw new InvalidOperationException("Report instance does not exist");
            var originalInstanceContents = original.ContentManyToMany.ToArray();

            var updatedContent = new Dictionary<long, Content>();
            // Delete removed content and add new content.
            originalInstanceContents.Except(entity.ContentManyToMany).ForEach(ric =>
            {
                this.Context.Entry(ric).State = EntityState.Deleted;
                // Content that is private will only exist on a single report.
                // If it's removed from the report we can remove it from the database.
                if (ric.Content?.IsPrivate == true) this.Context.Entry(ric.Content).State = EntityState.Deleted;
            });
            entity.ContentManyToMany.ForEach(ric =>
            {
                // Duplicate content can be in multiple sections, so we grab the first copy.
                var originalContent = originalInstanceContents.FirstOrDefault(o => o.ContentId == ric.ContentId)?.Content;
                var originalInstanceContent = originalInstanceContents.FirstOrDefault(o => o.ContentId == ric.ContentId && o.SectionName == ric.SectionName);
                if (originalInstanceContent == null)
                {
                    // If the content is new too, upload it to the database.
                    if (ric.Content?.Id == 0)
                    {
                        ric.Content.IsPrivate = true;
                        ric.Content.GuaranteeUid();
                        ric.Content.AddToContext(this.Context);
                        this.Context.Add(ric.Content);
                    }
                    else if (ric.Content != null)
                    {
                        // Fetch the original content from the database to compare.
                        originalContent = this.Context.Contents
                            .AsNoTracking()
                            .Include(c => c.TonePoolsManyToMany)
                            .FirstOrDefault(c => c.Id == ric.ContentId) ?? throw new InvalidOperationException($"Content in report instance does not exist {entity.Id}:{ric.ContentId}");

                        // Do not allow updating content not owned by the user.
                        // Content can be in more than one section.  Only the first copy that is modified will be used for the update.  All other copies will be synced.
                        if (!updatedContent.TryGetValue(ric.ContentId, out Content? reportContent) &&
                            originalContent.IsPrivate &&
                            (ric.Content.Headline != originalContent.Headline ||
                                ric.Content.Summary != originalContent.Summary ||
                                ric.Content.SourceId != originalContent.SourceId ||
                                ric.Content.OtherSource != originalContent.OtherSource ||
                                ric.Content.PublishedOn != originalContent.PublishedOn ||
                                ric.Content.Byline != originalContent.Byline ||
                                ric.Content.SourceUrl != originalContent.SourceUrl ||
                                ric.Content.Uid != originalContent.Uid ||
                                ric.Content.TonePoolsManyToMany.Any(tp =>
                                {
                                    var otp = originalContent.TonePoolsManyToMany.FirstOrDefault(otp => otp.TonePoolId == tp.TonePoolId);
                                    return otp?.Value != tp.Value;
                                })))
                        {
                            updatedContent.Add(ric.ContentId, ric.Content);
                            this.Context.Entry(ric.Content).State = EntityState.Modified;
                        }
                        else
                        {
                            // This content exists in another section and was updated so we need to make them the same content.
                            // If the client incorrectly submits the same content in more than one section and updates both of them differently, it will only use the first entry.
                            ric.Content = reportContent;
                        }
                    }

                    // Add new content to the report.
                    ric.Instance = null;
                    ric.InstanceId = entity.Id;
                    this.Context.Entry(ric).State = EntityState.Added;
                    original.ContentManyToMany.Add(ric);
                }
                else
                {
                    // Content was already in the report instance, now updated it.
                    if (ric.Content != null &&
                        originalContent != null &&
                        entity.OwnerId.HasValue)
                    {
                        if (originalContent.IsPrivate &&
                            (originalContent.Headline != ric.Content.Headline ||
                            originalContent.Summary != ric.Content.Summary ||
                            originalContent.Body != ric.Content.Body ||
                            originalContent.Byline != ric.Content.Byline ||
                            originalContent.Section != ric.Content.Section ||
                            originalContent.Page != ric.Content.Page ||
                            originalContent.SourceId != ric.Content.SourceId ||
                            originalContent.OtherSource != ric.Content.OtherSource ||
                            originalContent.SourceId != ric.Content.SourceId ||
                            originalContent.PublishedOn != ric.Content.PublishedOn ||
                            originalContent.Uid != ric.Content.Uid ||
                            originalContent.TonePoolsManyToMany.Any(tp =>
                            {
                                var ntp = ric.Content.TonePoolsManyToMany.FirstOrDefault(ntp => ntp.TonePoolId == tp.TonePoolId);
                                return ntp?.Value != tp.Value;
                            })))
                        {
                            // If the content belongs to the user and has changed, update it.
                            if (ric.Content.GuaranteeUid() && originalContent.Uid != ric.Content.Uid) originalContent.Uid = ric.Content.Uid;
                            this.Context.UpdateContext(originalContent, ric.Content);
                            this.Context.Update(originalContent);
                        }
                        else if (ric.Content.Versions.TryGetValue(entity.OwnerId.Value, out Entities.Models.ContentVersion? newVersion))
                        {
                            // Update the content versions if they have been provided.
                            if (originalContent.Versions.TryGetValue(entity.OwnerId.Value, out Entities.Models.ContentVersion? currentVersion))
                            {
                                // Check if this content has already been updated in another section.  If so ignore.
                                if (this.Context.Entry(originalContent).State != EntityState.Modified && (
                                    newVersion.Summary != currentVersion.Summary ||
                                    newVersion.Body != currentVersion.Body ||
                                    newVersion.Byline != currentVersion.Byline ||
                                    newVersion.Edition != currentVersion.Edition ||
                                    newVersion.Headline != currentVersion.Headline ||
                                    newVersion.Page != currentVersion.Page ||
                                    newVersion.Section != currentVersion.Section ||
                                    newVersion.OwnerId != currentVersion.OwnerId
                                ))
                                {
                                    // Only update the versions information.
                                    originalContent.Versions[entity.OwnerId.Value] = newVersion;
                                    this.Context.Update(originalContent);
                                    this.Context.Entry(originalContent).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                originalContent.Versions.Add(entity.OwnerId.Value, newVersion);
                                this.Context.Entry(originalContent).State = EntityState.Modified;
                            }
                        }
                    }
                    if (originalInstanceContent.SortOrder != ric.SortOrder)
                    {
                        originalInstanceContent.SortOrder = ric.SortOrder;
                        this.Context.Entry(originalInstanceContent).State = EntityState.Modified;
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
        else
            return base.Update(entity);
    }

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ReportInstance Update(ReportInstance entity)
    {
        return this.Update(entity, false);
    }

    /// <summary>
    /// Update the report instance in the context and the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    public ReportInstance UpdateAndSave(ReportInstance entity, bool updateChildren = false)
    {
        var report = this.Update(entity, updateChildren);
        this.CommitTransaction();
        return report;
    }


    public IEnumerable<UserReportInstance> GetUserReportInstances(long instanceId)
    {
        return this.Context.UserReportInstances
            .AsNoTracking()
            .Include(uri => uri.User)
            .Where(uri => uri.InstanceId == instanceId)
            .ToArray();
    }

    public UserReportInstance Add(UserReportInstance entity)
    {
        this.Context.Add(entity);
        return entity;
    }


    public UserReportInstance AddAndSave(UserReportInstance entity)
    {
        this.Add(entity);
        this.CommitTransaction();
        return entity;
    }

    public UserReportInstance Update(UserReportInstance entity)
    {
        this.Context.Update(entity);
        return entity;
    }

    public UserReportInstance UpdateAndSave(UserReportInstance entity)
    {
        this.Update(entity);
        this.CommitTransaction();
        return entity;
    }

    public IEnumerable<UserReportInstance> UpdateAndSave(IEnumerable<UserReportInstance> entities)
    {
        var instanceIds = entities.Select(e => e.InstanceId).Distinct().ToArray();
        var originalInstances = this.Context.UserReportInstances.Where(uri => instanceIds.Contains(uri.InstanceId)).ToArray();
        foreach (var entity in entities)
        {
            var original = originalInstances.FirstOrDefault(uri => uri.UserId == entity.UserId && uri.InstanceId == entity.InstanceId);
            if (original == null)
            {
                this.Context.Add(entity);
            }
            else
            {
                original.LinkStatus = entity.LinkStatus;
                original.LinkSentOn = entity.LinkSentOn;
                original.LinkResponse = entity.LinkResponse;
                original.TextStatus = entity.TextStatus;
                original.TextSentOn = entity.TextSentOn;
                original.TextResponse = entity.TextResponse;
                this.Context.Update(original);
            }
        }
        this.CommitTransaction();
        return entities;
    }

    public UserReportInstance Delete(UserReportInstance entity)
    {
        this.Context.Remove(entity);
        return entity;
    }

    public UserReportInstance DeleteAndSave(UserReportInstance entity)
    {
        this.Delete(entity);
        this.CommitTransaction();
        return entity;
    }
    #endregion
}
