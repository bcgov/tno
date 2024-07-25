using System.Globalization;
using System.Security.Claims;
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
        if (filter.IsSubscribedToReportId.HasValue)
            predicate = predicate.And(c => c.ReportSubscriptionsManyToMany.Any(rs => rs.IsSubscribed && rs.ReportId == filter.IsSubscribedToReportId.Value));

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
            .Where(u => u.Username.ToLower() == username.ToLower()).FirstOrDefault();
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
        entity.MediaTypesManyToMany.ForEach(mediaType =>
        {
            mediaType.User = entity;
            this.Context.Entry(mediaType).State = EntityState.Added;
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
        original.Version = entity.Version;
        original.Status = entity.Status;
        original.Note = entity.Note;
        original.Code = entity.Code;
        original.Roles = entity.Roles;
        original.Preferences = entity.Preferences;
        original.UniqueLogins = entity.UniqueLogins;
        original.LastLoginOn = entity.LastLoginOn;
        if (String.IsNullOrWhiteSpace(entity.Code)) original.CodeCreatedOn = null;
        else if (original.Code != entity.Code) original.CodeCreatedOn = DateTime.UtcNow;

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

        base.UpdateAndSave(original);
        return FindById(entity.Id)!;
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

        // Unlink
        base.Delete(entity);
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
                        this.Context.Add(new UserProduct(account.ToAccountId, transfer.OriginalId, true));
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
                        this.Context.Add(new UserProduct(account.ToAccountId, copy.OriginalId, true));
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
    #endregion
}
