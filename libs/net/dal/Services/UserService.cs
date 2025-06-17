using System.Security.Claims;
using System.Text.Json;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class UserService : BaseService<User, int>, IUserService
{
    #region Properties
    #endregion

    #region Constructors
    public UserService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<UserService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<User> FindAll()
    {
        return this.Context.Users
            .AsNoTracking()
            .OrderBy(a => a.Username).ThenBy(a => a.LastName).ThenBy(a => a.FirstName).ToArray();
    }

    public IPaged<User> Find(UserFilter filter)
    {
        var query = this.Context.Users
            .AsNoTracking()
            .AsQueryable();

        var predicate = PredicateBuilder.New<User>(true);

        if (!String.IsNullOrWhiteSpace(filter.Username))
            predicate = predicate.And(c => EF.Functions.Like(c.Username.ToLower(), $"{filter.Username.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Email))
            predicate = predicate.And(c => EF.Functions.Like(c.Email.ToLower(), $"{filter.Email.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Name))
            predicate = predicate.And(c => EF.Functions.Like(c.FirstName.ToLower(), $"{filter.Name.ToLower()}%") || EF.Functions.Like(c.LastName.ToLower(), $"{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.FirstName))
            predicate = predicate.And(c => EF.Functions.Like(c.FirstName.ToLower(), $"{filter.FirstName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.LastName))
            predicate = predicate.And(c => EF.Functions.Like(c.LastName.ToLower(), $"{filter.LastName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLower();
            predicate = predicate.And(c => EF.Functions.Like(c.Username.ToLower(), $"%{keyword}%")
                || EF.Functions.Like(c.Email.ToLower(), $"%{keyword}%")
                || EF.Functions.Like(c.PreferredEmail.ToLower(), $"%{keyword}%")
                || EF.Functions.Like(c.FirstName.ToLower(), $"%{keyword}%")
                || EF.Functions.Like(c.LastName.ToLower(), $"%{keyword}%"));
        }
        if (!String.IsNullOrWhiteSpace(filter.RoleName))
            predicate = predicate.And(c => EF.Functions.Like(c.Roles.ToLower(), $"%[{filter.RoleName.ToLower()}]%"));

        if (filter.Status != null)
            predicate = predicate.And(c => c.Status == filter.Status);
        if (filter.IsEnabled != null)
            predicate = predicate.And(c => c.IsEnabled == filter.IsEnabled);
        if (filter.IsSystemAccount != null)
            predicate = predicate.And(c => c.IsSystemAccount == filter.IsSystemAccount);
        if (filter.AccountTypes?.Any() == true)
            predicate = predicate.And(c => filter.AccountTypes.Contains(c.AccountType));
        if (filter.IsSubscribedToProductId.HasValue)
        {
            // Only return users who are subscribed to the specified product.
            var product = this.Context.Products.FirstOrDefault(p => p.Id == filter.IsSubscribedToProductId.Value);
            if (product != null)
            {
                predicate = predicate.And(c => c.ProductSubscriptionsManyToMany.Any(s => s.ProductId == product.Id));
            }
        }
        if (filter.IsSubscribedToReportId.HasValue)
            predicate = predicate.And(u => u.ReportSubscriptionsManyToMany.Any(s => s.IsSubscribed && s.ReportId == filter.IsSubscribedToReportId));
        if (filter.IsSubscribedToNotificationId.HasValue)
            predicate = predicate.And(u => u.NotificationSubscriptionsManyToMany.Any(s => s.IsSubscribed && s.NotificationId == filter.IsSubscribedToNotificationId));
        if (filter.IsSubscribedToEveningOverview.HasValue)
            predicate = predicate.And(u => u.AVOverviewSubscriptionsManyToMany.Any(s => s.IsSubscribed && s.TemplateType == filter.IsSubscribedToEveningOverview));

        if (filter.IncludeUserId.HasValue)
            predicate = PredicateBuilder.Or<User>(u => u.Id == filter.IncludeUserId, predicate);

        query = query.Where(predicate);
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
            query = query.OrderBy(u => u.Status).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.Username);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query
            .Skip(skip)
            .Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<User>();
        return new Paged<User>(items, page, quantity, total);
    }

    public override User? FindById(int id)
    {
        return this.Context.Users
            .Include(u => u.MediaTypesManyToMany)
            .Include(u => u.SourcesManyToMany)
            .Include(u => u.OrganizationsManyToMany).ThenInclude(o => o.Organization)
            .Include(u => u.ReportSubscriptionsManyToMany).ThenInclude(o => o.Report)
            .FirstOrDefault(u => u.Id == id);
    }

    public User? FindByUserKey(string key)
    {
        return this.Context.Users
            .Where(u => u.Key == key).FirstOrDefault();
    }

    public User? FindByUsername(string username)
    {
        return this.Context.Users
            .Include(u => u.MediaTypes)
            .Include(u => u.Sources)
            .Where(u => u.Username.ToLower() == username.ToLower())
            .FirstOrDefault();
    }

    public IEnumerable<User> FindByEmail(string email)
    {
        return this.Context.Users
            .Where(u => u.Email.ToLower() == email.ToLower() || u.PreferredEmail.ToLower() == email.ToLower());
    }

    public override User AddAndSave(User entity)
    {
        entity.SourcesManyToMany.ForEach(source =>
        {
            source.User = entity;
            this.Context.Entry(source).State = EntityState.Added;
        });
        entity.OrganizationsManyToMany.ForEach(organization =>
        {
            organization.User = entity;
            this.Context.Entry(organization).State = EntityState.Added;
        });
        entity.MediaTypesManyToMany.ForEach(mediaType =>
        {
            mediaType.User = entity;
            this.Context.Entry(mediaType).State = EntityState.Added;
        });
        entity.Distribution.ForEach(user =>
        {
            user.User = entity;
            this.Context.Entry(user).State = EntityState.Added;
        });

        base.AddAndSave(entity);
        return FindById(entity.Id)!;
    }

    public override User UpdateAndSave(User entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException();
        original.AccountType = entity.AccountType;
        original.Key = entity.Key;
        original.Username = entity.Username;
        original.Email = entity.Email;
        original.PreferredEmail = entity.PreferredEmail;
        original.DisplayName = entity.DisplayName;
        original.EmailVerified = entity.EmailVerified;
        original.IsEnabled = entity.IsEnabled;
        original.FirstName = entity.FirstName;
        original.LastName = entity.LastName;
        original.Status = entity.Status;
        original.Note = entity.Note;
        original.Code = entity.Code;
        original.Roles = entity.Roles;
        original.UniqueLogins = entity.UniqueLogins;

        // Always allow updating.
        original.Version = entity.Version;

        if (String.IsNullOrWhiteSpace(entity.Code)) original.CodeCreatedOn = null;
        else if (original.Code != entity.Code) original.CodeCreatedOn = DateTime.UtcNow;

        // update SourcesManyToMany
        var originalSources = this.Context.UserSources.Where(us => us.UserId == entity.Id).ToArray();
        originalSources.Except(entity.SourcesManyToMany).ForEach((source) =>
        {
            this.Context.Entry(source).State = EntityState.Deleted;
        });
        entity.SourcesManyToMany.ForEach((source) =>
        {
            var originalSource = originalSources.FirstOrDefault(s => s.SourceId == source.SourceId);
            if (originalSource == null)
            {
                source.UserId = original.Id;
                this.Context.Entry(source).State = EntityState.Added;
            }
        });

        // update MediaTypesManyToMany
        var originalMediaTypes = this.Context.UserMediaTypes.Where(umt => umt.UserId == entity.Id).ToArray();
        originalMediaTypes.Except(entity.MediaTypesManyToMany).ForEach((mediaType) =>
        {
            this.Context.Entry(mediaType).State = EntityState.Deleted;
        });
        entity.MediaTypesManyToMany.ForEach((mediaType) =>
        {
            var originalMediaType = originalMediaTypes.FirstOrDefault(s => s.MediaTypeId == mediaType.MediaTypeId);
            if (originalMediaType == null)
            {
                mediaType.UserId = original.Id;
                this.Context.Entry(mediaType).State = EntityState.Added;
            }
        });

        // update OrganizationsManyToMany
        var originalOrganizations = this.Context.UserOrganizations.Where(umt => umt.UserId == entity.Id).ToArray();
        originalOrganizations.Except(entity.OrganizationsManyToMany).ForEach((org) =>
        {
            this.Context.Entry(org).State = EntityState.Deleted;
        });
        entity.OrganizationsManyToMany.ForEach((org) =>
        {
            var originalOrganization = originalOrganizations.FirstOrDefault(s => s.OrganizationId == org.OrganizationId);
            if (originalOrganization == null)
            {
                org.UserId = original.Id;
                this.Context.Entry(org).State = EntityState.Added;
            }
        });

        // update ReportSubscriptionsManyToMany
        var originalReports = this.Context.UserReports.Where(umt => umt.UserId == entity.Id).ToArray();
        originalReports.Except(entity.ReportSubscriptionsManyToMany).ForEach((org) =>
        {
            this.Context.Entry(org).State = EntityState.Deleted;
        });
        entity.ReportSubscriptionsManyToMany.ForEach((org) =>
        {
            var originalReport = originalReports.FirstOrDefault(s => s.ReportId == org.ReportId);
            if (originalReport == null)
            {
                org.UserId = original.Id;
                this.Context.Entry(org).State = EntityState.Added;
            }
        });

        base.UpdateAndSave(original);
        return FindById(entity.Id)!;
    }

    public User UpdateDistributionList(User entity)
    {
        var originalDistributions = this.Context.UserDistributions.Where(d => d.UserId == entity.Id).ToArray();
        originalDistributions.Except(entity.Distribution).ForEach((user) =>
        {
            this.Context.Entry(user).State = EntityState.Deleted;
        });
        entity.Distribution.ForEach((user) =>
        {
            var originalDistribution = originalDistributions.FirstOrDefault(s => s.LinkedUserId == user.LinkedUserId);
            if (originalDistribution == null)
            {
                user.UserId = entity.Id;
                this.Context.Entry(user).State = EntityState.Added;
            }
        });

        entity = this.UpdateAndSave(entity);
        entity.Distribution.Clear();
        entity.Distribution.AddRange(this.Context.UserDistributions.Include(d => d.LinkedUser).Where(d => d.UserId == entity.Id));
        return entity;
    }

    public User UpdatePreferences(User model)
    {
        var original = FindById(model.Id) ?? throw new NoContentException();
        original.Preferences = model.Preferences;
        base.UpdateAndSave(original);
        return FindById(model.Id)!;
    }

    public IEnumerable<User> FindByRoles(IEnumerable<string> roles)
    {
        var result = Context.Users.AsNoTracking();
        if (roles.Any())
        {
            var predicate = PredicateBuilder.New<User>();
            foreach (var role in roles)
            {
                var currentRole = role;
                predicate = predicate.Or(x => x.Roles.ToLower().Contains(currentRole));
            }
            result = result.Where(predicate);
        }
        return result.OrderBy(a => a.Username).ThenBy(a => a.LastName).ThenBy(a => a.FirstName);
    }

    /// <summary>
    /// Delete the user
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(User entity)
    {
        // Unlink work orders.
        var workOrders = this.Context.WorkOrders.Where(wo => wo.AssignedId == entity.Id || wo.RequestorId == entity.Id);
        workOrders.ForEach(wo =>
        {
            if (wo.AssignedId == entity.Id) wo.AssignedId = null;
            if (wo.RequestorId == entity.Id) wo.RequestorId = null;
            this.Context.Update(wo);
        });

        // We want to delete the user even if the account version is old.
        var original = this.Context.Users.FirstOrDefault(u => u.Id == entity.Id) ?? throw new NoContentException();

        // Unlink
        base.Delete(original);
    }

    /// <summary>
    /// Transfer the ownership of the specified account objects to the specified user.
    /// Or copy the specified account objects to the specified user.
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public User? TransferAccount(API.Areas.Admin.Models.User.TransferAccountModel account)
    {
        var user = this.FindById(account.ToAccountId);

        if (account.TransferOwnership)
        {
            foreach (var transfer in account.Notifications.Where(t => t.Checked))
            {
                var item = this.Context.Notifications.FirstOrDefault(n => n.Id == transfer.OriginalId);
                if (item != null)
                {
                    var subscriptions = this.Context.UserNotifications.Where(un => un.NotificationId == transfer.OriginalId && (un.UserId == account.ToAccountId || un.UserId == account.FromAccountId));
                    var isToSubscribed = subscriptions.Any(s => s.UserId == account.ToAccountId);
                    var fromSubscription = subscriptions.FirstOrDefault(s => s.UserId == account.FromAccountId);
                    if (!isToSubscribed)
                    {
                        this.Context.Add(new UserNotification(account.ToAccountId, transfer.OriginalId, true, null));
                    }
                    if (fromSubscription != null)
                    {
                        this.Context.Remove(fromSubscription);
                    }
                    if (!transfer.SubscribeOnly)
                    {
                        // Transfer the ownership of the object to the new account.
                        if (!String.IsNullOrWhiteSpace(transfer.NewName) && transfer.NewName != item.Name) item.Name = transfer.NewName;
                        item.OwnerId = account.ToAccountId;
                        this.Context.Update(item);
                    }
                }
            }
            foreach (var transfer in account.Reports.Where(t => t.Checked))
            {
                var item = this.Context.Reports.FirstOrDefault(n => n.Id == transfer.OriginalId);
                if (item != null)
                {
                    var subscriptions = this.Context.UserReports.Where(un => un.ReportId == transfer.OriginalId && (un.UserId == account.ToAccountId || un.UserId == account.FromAccountId));
                    var isToSubscribed = subscriptions.Any(s => s.UserId == account.ToAccountId);
                    var fromSubscription = subscriptions.FirstOrDefault(s => s.UserId == account.FromAccountId);
                    if (!isToSubscribed)
                    {
                        this.Context.Add(new UserReport(account.ToAccountId, transfer.OriginalId, true));
                    }
                    if (fromSubscription != null)
                    {
                        this.Context.Remove(fromSubscription);
                    }
                    if (!transfer.SubscribeOnly)
                    {
                        // Transfer the ownership of the object to the new account.
                        if (!String.IsNullOrWhiteSpace(transfer.NewName) && transfer.NewName != item.Name) item.Name = transfer.NewName;
                        item.OwnerId = account.ToAccountId;
                        this.Context.Update(item);
                    }
                }
            }
            foreach (var transfer in account.Products.Where(t => t.Checked))
            {
                var item = this.Context.Products.FirstOrDefault(n => n.Id == transfer.OriginalId);
                if (item != null)
                {
                    var subscriptions = this.Context.UserProducts.Where(un => un.ProductId == transfer.OriginalId && (un.UserId == account.ToAccountId || un.UserId == account.FromAccountId));
                    var isToSubscribed = subscriptions.Any(s => s.UserId == account.ToAccountId);
                    var fromSubscription = subscriptions.FirstOrDefault(s => s.UserId == account.FromAccountId);
                    if (!isToSubscribed)
                    {
                        this.Context.Add(new UserProduct(account.ToAccountId, transfer.OriginalId));
                    }
                    if (fromSubscription != null)
                    {
                        this.Context.Remove(fromSubscription);
                    }
                }
            }
            foreach (var transfer in account.Filters.Where(t => t.Checked))
            {
                var item = this.Context.Filters.FirstOrDefault(n => n.Id == transfer.OriginalId);
                if (item != null)
                {
                    // Transfer the ownership of the object to the new account.
                    if (!String.IsNullOrWhiteSpace(transfer.NewName) && transfer.NewName != item.Name) item.Name = transfer.NewName;
                    item.OwnerId = account.ToAccountId;
                    this.Context.Update(item);
                }
            }
            foreach (var transfer in account.Folders.Where(t => t.Checked))
            {
                var item = this.Context.Folders.FirstOrDefault(n => n.Id == transfer.OriginalId);
                if (item != null)
                {
                    // Transfer the ownership of the object to the new account.
                    if (!String.IsNullOrWhiteSpace(transfer.NewName) && transfer.NewName != item.Name) item.Name = transfer.NewName;
                    item.OwnerId = account.ToAccountId;
                    this.Context.Update(item);
                }
            }

            // Transfer history to new account.
            if (account.IncludeHistory)
            {
                var sqlParams = new object[] {
                    new Npgsql.NpgsqlParameter("fromAccountId", account.FromAccountId),
                    new Npgsql.NpgsqlParameter("toAccountId", account.ToAccountId),
                };
                this.Context.Database.ExecuteSqlRaw(
                    @$"UPDATE public.""notification_instance""
                    SET ""owner_id"" = @toAccountId
                    WHERE ""owner_id"" = @fromAccountId;", sqlParams);
                this.Context.Database.ExecuteSqlRaw(
                    @$"UPDATE public.""report_instance""
                    SET ""owner_id"" = @toAccountId
                    WHERE ""owner_id"" = @fromAccountId;", sqlParams);
            }
        }
        else
        {
            var folders = new Dictionary<int, Folder>();
            var filters = new Dictionary<int, Filter>();
            var reports = new Dictionary<int, Report>();

            // Copy the objects.  This requires remapping foreign keys.
            foreach (var copy in account.Notifications.Where(t => t.Checked))
            {
                var item = this.Context.Notifications
                    .Include(n => n.Schedules)
                    .ThenInclude(s => s.Schedule)
                    .Include(n => n.SubscribersManyToMany)
                    .FirstOrDefault(n => n.Id == copy.OriginalId);
                if (item != null)
                {
                    if (copy.SubscribeOnly && !item.SubscribersManyToMany.Any(ns => ns.UserId == account.ToAccountId))
                    {
                        // Only subscribe if not already subscribed.
                        this.Context.Add(new UserNotification(account.ToAccountId, copy.OriginalId, true, null));
                    }
                    else if (!copy.SubscribeOnly)
                    {
                        // Copy the object to the new account.
                        var newItem = new Notification(item, account.ToAccountId)
                        {
                            Name = !String.IsNullOrWhiteSpace(copy.NewName) && copy.NewName != item.Name ? copy.NewName! : item.Name,
                        };
                        this.Context.Add(newItem);
                    }
                }
            }
            foreach (var copy in account.Products.Where(t => t.Checked))
            {
                var item = this.Context.Products.FirstOrDefault(n => n.Id == copy.OriginalId);
                if (item != null)
                {
                    var isToSubscribed = this.Context.UserProducts.Any(un => un.ProductId == copy.OriginalId && (un.UserId == account.ToAccountId));
                    if (!isToSubscribed)
                    {
                        this.Context.Add(new UserProduct(account.ToAccountId, copy.OriginalId));
                    }
                }
            }
            foreach (var copy in account.Filters.Where(t => t.Checked))
            {
                var item = this.Context.Filters
                    .FirstOrDefault(n => n.Id == copy.OriginalId);
                if (item != null)
                {
                    if (!copy.SubscribeOnly)
                    {
                        // Copy the object to the new account.
                        var newItem = new Filter(item, account.ToAccountId)
                        {
                            Name = !String.IsNullOrWhiteSpace(copy.NewName) && copy.NewName != item.Name ? copy.NewName! : item.Name,
                        };
                        filters.Add(copy.OriginalId, newItem);
                        this.Context.Add(newItem);
                    }
                }
            }
            foreach (var copy in account.Folders.Where(t => t.Checked))
            {
                var item = this.Context.Folders
                    .Include(f => f.ContentManyToMany)
                    .FirstOrDefault(n => n.Id == copy.OriginalId);
                if (item != null)
                {
                    if (!copy.SubscribeOnly)
                    {
                        var linkedFilter = item.FilterId.HasValue && filters.TryGetValue(item.FilterId.Value, out Filter? filter) ? filter : null;
                        // Copy the object to the new account.
                        var newItem = new Folder(item, account.ToAccountId, linkedFilter)
                        {
                            Name = !String.IsNullOrWhiteSpace(copy.NewName) && copy.NewName != item.Name ? copy.NewName! : item.Name,
                        };
                        folders.Add(copy.OriginalId, newItem);
                        this.Context.Add(newItem);
                    }
                }
            }
            foreach (var copy in account.Reports.Where(t => t.Checked))
            {
                var item = this.Context.Reports
                    .Include(r => r.Sections)
                    .Include(n => n.SubscribersManyToMany)
                    .FirstOrDefault(n => n.Id == copy.OriginalId);
                if (item != null)
                {
                    if (copy.SubscribeOnly && !item.SubscribersManyToMany.Any(ns => ns.UserId == account.ToAccountId))
                    {
                        this.Context.Add(new UserReport(account.ToAccountId, copy.OriginalId, true));
                    }
                    else if (!copy.SubscribeOnly)
                    {
                        // Copy the object to the new account.
                        var newItem = new Report(item, account.ToAccountId)
                        {
                            Name = !String.IsNullOrWhiteSpace(copy.NewName) && copy.NewName != item.Name ? copy.NewName! : item.Name,
                        };
                        reports.Add(copy.OriginalId, newItem);
                        this.Context.Add(newItem);
                    }
                }
            }
            // Must reiterate through reports to remap linked reports within sections.
            foreach (var copy in reports.Values)
            {
                foreach (var section in copy.Sections)
                {
                    // Remap the filter/folder/report in each section.
                    if (section.FilterId.HasValue && filters.TryGetValue(section.FilterId.Value, out Filter? filter))
                    {
                        section.FilterId = filter.Id;
                        section.Filter = filter;
                    }
                    if (section.FolderId.HasValue && folders.TryGetValue(section.FolderId.Value, out Folder? folder))
                    {
                        section.FolderId = folder.Id;
                        section.Folder = folder;
                    }
                    if (section.LinkedReportId.HasValue && reports.TryGetValue(section.LinkedReportId.Value, out Report? report))
                    {
                        section.LinkedReportId = report.Id;
                        section.LinkedReport = report;
                    }
                }
            }
        }

        this.CommitTransaction();

        return user;
    }

    /// <summary>
    /// Get all the users in the specified distribution list.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<User> GetDistributionList(int userId)
    {
        return (from ud in this.Context.UserDistributions
                join u in this.Context.Users on ud.LinkedUserId equals u.Id
                where ud.UserId == userId
                select u).ToArray();
    }

    /// <summary>
    /// Get all product subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<UserProduct> GetUserProductSubscriptions(int userId)
    {
        // Get all distribution lists this user is part of.
        var distributionIds = this.Context.UserDistributions
            .Where(ud => ud.LinkedUserId == userId)
            .Select(ud => ud.UserId).ToArray();

        return this.Context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Product)
            .Include(up => up.User).ThenInclude(u => u!.NotificationSubscriptionsManyToMany)
            .Include(up => up.User).ThenInclude(u => u!.ReportSubscriptionsManyToMany)
            .Include(up => up.User).ThenInclude(u => u!.AVOverviewSubscriptionsManyToMany)
            .Where(up => up.UserId == userId &&
                ((up.Product!.ProductType == ProductType.Report &&
                    this.Context.UserReports.Any(ur => ur.ReportId == up.Product.TargetProductId &&
                        ur.IsSubscribed &&
                        (ur.UserId == userId || distributionIds.Contains(ur.UserId)))) ||
                (up.Product!.ProductType == ProductType.Notification &&
                    this.Context.UserNotifications.Any(un => un.NotificationId == up.Product.TargetProductId &&
                        un.IsSubscribed && (un.UserId == userId || distributionIds.Contains(un.UserId)))) ||
                (up.Product!.ProductType == ProductType.EveningOverview &&
                    this.Context.UserAVOverviews.Any(av => av.IsSubscribed && (av.UserId == userId || distributionIds.Contains(av.UserId)))))
            )
            .OrderBy(up => up.Product!.Name)
            .ToArray();
    }

    /// <summary>
    /// Get all report subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<UserReport> GetUserReportSubscriptions(int userId)
    {
        // Get all distribution lists this user is part of.
        var distributionIds = this.Context.UserDistributions
            .Where(ud => ud.LinkedUserId == userId)
            .Select(ud => ud.UserId).ToArray();

        return this.Context.UserReports
            .Include(ur => ur.Report)
            .Where(ur => ur.IsSubscribed && (ur.UserId == userId || distributionIds.Contains(ur.UserId)))
            .OrderBy(up => up.Report!.Name)
            .ToArray();
    }

    /// <summary>
    /// Get all evening overview subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<UserAVOverview> GetUserEveningOverviewSubscriptions(int userId)
    {
        // Get all distribution lists this user is part of.
        var distributionIds = this.Context.UserDistributions
            .Where(ud => ud.LinkedUserId == userId)
            .Select(ud => ud.UserId).ToArray();

        return this.Context.UserAVOverviews
            .Where(ur => ur.UserId == userId || distributionIds.Contains(ur.UserId))
            .OrderBy(up => up.TemplateType)
            .ToArray();
    }

    /// <summary>
    /// Get all notification subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<UserNotification> GetUserNotificationSubscriptions(int userId)
    {
        // Get all distribution lists this user is part of.
        var distributionIds = this.Context.UserDistributions
            .Where(ud => ud.LinkedUserId == userId)
            .Select(ud => ud.UserId).ToArray();

        return this.Context.UserNotifications
            .Include(un => un.Notification)
            .Where(un => un.IsSubscribed && (un.UserId == userId || distributionIds.Contains(un.UserId)))
            .OrderBy(up => up.Notification!.Name)
            .ToArray();
    }
    #endregion
}
