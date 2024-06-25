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

        var predicate = PredicateBuilder.New<User>();

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
        else
            predicate = predicate.And(c => !c.IsSystemAccount);
        if (filter.AccountType != null)
            predicate = predicate.And(c => c.AccountType == filter.AccountType);

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

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query
            .Skip(skip)
            .Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<User>();
        return new Paged<User>(items, filter.Page, filter.Quantity, total);
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
    #endregion
}
