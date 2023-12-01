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

public class UserColleagueService : BaseService<UserColleague, int>, IUserColleagueService
{
    #region Properties
    #endregion

    #region Constructors
    public UserColleagueService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<UserService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public UserColleague? FindColleagueByKey(int userId, int colleagueId)
    {
        return this.Context.UserColleagues
        .Include(x => x.Colleague)
            .Where(u => u.UserId == userId && u.ColleagueId == colleagueId).FirstOrDefault();
    }

    public IEnumerable<UserColleague> FindColleaguesByUserId(int id)
    {
        var test = this.Context.UserColleagues
        .Include(x => x.Colleague)
            .Where(u => u.UserId == id);
        return test;
    }

    public UserColleague AddColleague(UserColleague userColleague)
    {
        base.AddAndSave(userColleague);
        return userColleague;
    }

    public IEnumerable<UserColleague> FindColleaguesByEmail(string email)
    {
        throw new NotImplementedException();
    }

    public UserColleague RemoveColleague(int userId, int colleagueId)
    {
        var userColleague = FindColleagueByKey(userId, colleagueId);
        if (userColleague != null)
        {
          base.DeleteAndSave(userColleague);
        }
        return userColleague;
    }

    public UserColleague UpdateColleague(UserColleague userColleague)
    {
        base.UpdateAndSave(userColleague);
        return userColleague;
    }
    #endregion
}
