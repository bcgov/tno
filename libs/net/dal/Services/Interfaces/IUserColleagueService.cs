
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IUserColleagueService : IBaseService<UserColleague, int>
{
    IEnumerable<UserColleague> FindColleaguesByEmail(string email);
    IEnumerable<UserColleague> FindColleaguesByUserId(int id);
    UserColleague AddColleague(UserColleague userColleague);
    UserColleague? RemoveColleague(int userId, int colleagueId);
}
