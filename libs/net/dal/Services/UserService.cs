using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

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

        if (!String.IsNullOrWhiteSpace(filter.Username))
            query = query.Where(c => EF.Functions.Like(c.Username.ToLower(), $"{filter.Username.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(c => EF.Functions.Like(c.Email.ToLower(), $"{filter.Email.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.FirstName.ToLower(), $"{filter.Name.ToLower()}%") || EF.Functions.Like(c.LastName.ToLower(), $"{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.FirstName))
            query = query.Where(c => EF.Functions.Like(c.FirstName.ToLower(), $"{filter.FirstName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.LastName))
            query = query.Where(c => EF.Functions.Like(c.LastName.ToLower(), $"{filter.LastName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLower();
            query = query.Where(c => EF.Functions.Like(c.Username.ToLower(), $"{keyword}%") || EF.Functions.Like(c.Email.ToLower(), $"{keyword}%") || EF.Functions.Like(c.FirstName.ToLower(),
            $"{keyword}%") || EF.Functions.Like(c.LastName.ToLower(), $"{keyword}%"));
        }
        if (!String.IsNullOrWhiteSpace(filter.RoleName))
            query = query.Where(c => EF.Functions.Like(c.Roles.ToLower(), $"%[{filter.RoleName.ToLower()}]%"));

        if (filter.Status != null)
            query = query.Where(c => c.Status == filter.Status);
        if (filter.IsEnabled != null)
            query = query.Where(c => c.IsEnabled == filter.IsEnabled);
        if (filter.IsSystemAccount != null)
        {
            query = query.Where(c => c.IsSystemAccount == filter.IsSystemAccount);
        }
        else
        {
            query = query.Where(c => !c.IsSystemAccount);
        }

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
            .FirstOrDefault(c => c.Id == id);
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
            .Where(u => u.Email.ToLower() == email.ToLower());
    }

    public override User AddAndSave(User entity)
    {
        base.AddAndSave(entity);
        return FindById(entity.Id)!;
    }

    public override User UpdateAndSave(User entity)
    {
        var original = FindById(entity.Id);

        if (original != null)
        {
            original.Key = entity.Key;
            original.Username = entity.Username;
            original.Email = entity.Email;
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
            original.LastLoginOn = entity.LastLoginOn;
            if (String.IsNullOrWhiteSpace(entity.Code)) original.CodeCreatedOn = null;
            else if (original.Code != entity.Code) original.CodeCreatedOn = DateTime.UtcNow;

            base.UpdateAndSave(original);
            return FindById(entity.Id)!;
        }

        throw new InvalidOperationException("User does not exist");
    }
    #endregion
}
