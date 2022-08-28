using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class ContentService : BaseService<Content, long>, IContentService
{
    #region Properties
    private readonly string? invalidEncodings;
    private readonly string[]? encodingSets;
    #endregion

    #region Constructors
    public ContentService(TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ContentService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        invalidEncodings = configuration.GetValue<string>("InvalidEncodings");
        encodingSets = invalidEncodings?.Split("__", StringSplitOptions.RemoveEmptyEntries);
    }
    #endregion

    #region Methods

    public IPaged<Content> Find(ContentFilter filter)
    {
        var query = this.Context.Contents
            .Include(c => c.ContentType)
            .Include(c => c.MediaType)
            .Include(c => c.DataSource)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.Owner)
            .Include(c => c.PrintContent)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Source))
            query = query.Where(c => c.Source == filter.Source);
        if (!String.IsNullOrWhiteSpace(filter.Headline))
            query = query.Where(c => EF.Functions.Like(c.Headline.ToLower(), $"%{filter.Headline.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.PageName))
            query = query.Where(c => EF.Functions.Like(c.Page.ToLower(), $"%{filter.PageName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Section))
            query = query.Where(c => c.PrintContent != null && EF.Functions.Like(c.PrintContent.Section.ToLower(), $"%{filter.Section.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Edition))
            query = query.Where(c => c.PrintContent != null && EF.Functions.Like(c.PrintContent.Edition.ToLower(), $"%{filter.Edition.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.StoryType))
            query = query.Where(c => c.PrintContent != null && EF.Functions.Like(c.PrintContent.StoryType.ToLower(), $"%{filter.StoryType.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Byline))
            query = query.Where(c => c.PrintContent != null && EF.Functions.Like(c.PrintContent.Byline.ToLower(), $"%{filter.Byline.ToLower()}%"));

        if (!String.IsNullOrWhiteSpace(filter.MediaType))
            query = query.Where(c => EF.Functions.Like(c.MediaType!.Name, $"%{filter.MediaType}%"));

        if (filter.ContentTypeId.HasValue)
            query = query.Where(c => c.ContentTypeId == filter.ContentTypeId);
        if (filter.MediaTypeId.HasValue)
            query = query.Where(c => c.MediaTypeId == filter.MediaTypeId);
        if (filter.DataSourceId.HasValue)
            query = query.Where(c => c.DataSourceId == filter.DataSourceId);
        if (filter.OwnerId.HasValue)
            query = query.Where(c => c.OwnerId == filter.OwnerId);
        if (filter.UserId.HasValue)
        {
            var user = this.Context.Users.Find(filter.UserId);
            if (user != null)
                query = query.Where(c => c.OwnerId == user.Id || c.CreatedById == user.Key || c.UpdatedById == user.Key || c.Logs.Any(l => l.CreatedById == user.Key));
        }

        if (filter.CreatedOn.HasValue)
            query = query.Where(c => c.CreatedOn == filter.CreatedOn.Value.ToUniversalTime());
        if (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime() && c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());
        else if (filter.CreatedStartOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime());
        else if (filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());

        if (filter.UpdatedOn.HasValue)
            query = query.Where(c => c.UpdatedOn == filter.UpdatedOn.Value.ToUniversalTime());
        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime() && c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());
        else if (filter.UpdatedStartOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime());
        else if (filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());

        if (filter.PublishedOn.HasValue)
            query = query.Where(c => c.PublishedOn == filter.PublishedOn.Value.ToUniversalTime());
        if (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime() && c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());
        else if (filter.PublishedStartOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime());
        else if (filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());

        if (filter.Actions.Any() == true)
            query = query.Where(c => c.ActionsManyToMany.Any(ca => filter.Actions.Contains(ca.Action!.Name)
                && ((ca.Action.ValueType == Entities.ValueType.Boolean && ca.Value == "true")
                    || (ca.Action.ValueType != Entities.ValueType.Boolean && !String.IsNullOrWhiteSpace(ca.Value)))));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderByDescending(c => c.Id);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Content>();

        HandleInvalidEncoding(items);

        return new Paged<Content>(items, filter.Page, filter.Quantity, total);
    }

    private void HandleInvalidEncoding(params Content[] contents)
    {
        if (contents != null && contents.Length > 0 &&
            !string.IsNullOrEmpty(invalidEncodings) && encodingSets != null)
        {
            foreach (var encodingSet in encodingSets)
            {
                var keyValues = encodingSet.Split(":_", StringSplitOptions.RemoveEmptyEntries);
                if (keyValues?.Length == 2)
                {
                    var newValue = keyValues[0];
                    var oldValues = keyValues[1].Split("_", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var oldValue in oldValues)
                    {
                        foreach (var content in contents)
                        {
                            content.Headline = content.Headline.Replace(oldValue, newValue);
                            content.Summary = content.Summary.Replace(oldValue, newValue);
                        }
                    }
                }
            }
        }
    }

    public override Content? FindById(long id)
    {
        var result = this.Context.Contents
            .Include(c => c.ContentType)
            .Include(c => c.MediaType)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.DataSource)
            .Include(c => c.Owner)
            .Include(c => c.PrintContent)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.CategoriesManyToMany).ThenInclude(cc => cc.Category)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .FirstOrDefault(c => c.Id == id);

        if (result != null) HandleInvalidEncoding(result);
        return result;
    }

    public Content? FindByUid(string uid, string? source)
    {
        var query = this.Context.Contents
            .Include(c => c.ContentType)
            .Include(c => c.MediaType)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.DataSource)
            .Include(c => c.Owner)
            .Include(c => c.PrintContent)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.CategoriesManyToMany).ThenInclude(cc => cc.Category)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .Where(c => c.Uid == uid);

        if (!String.IsNullOrWhiteSpace(source))
            query = query.Where(c => c.Source == source);

        var result = query.FirstOrDefault();
        if (result != null) HandleInvalidEncoding(result);
        return result;
    }

    public override Content Add(Content entity)
    {
        entity.AddToContext(this.Context);
        base.Add(entity);
        return entity;
    }

    public override Content Update(Content entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity);
        base.Update(original);
        return original;
    }
    #endregion
}
