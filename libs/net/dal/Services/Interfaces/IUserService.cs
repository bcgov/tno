
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IUserService : IBaseService<User, int>
{
    IEnumerable<User> FindAll();

    IPaged<User> Find(UserFilter filter);

    User? FindByUserKey(string key);

    IEnumerable<User> GetUserUpdateHistory();

    User UpdateDistributionList(User entity);

    User UpdatePreferences(User user);

    User? FindByUsername(string username);
    IEnumerable<User> FindByEmail(string email);
    IEnumerable<User> FindByRoles(IEnumerable<string> roles);

    /// <summary>
    /// Find user by ID with minimal data (optimized for performance).
    /// Only returns basic user information without related entities.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User with minimal data or null if not found</returns>
    User? FindByIdMinimal(int id);
    User? TransferAccount(API.Areas.Admin.Models.User.TransferAccountModel account);

    IEnumerable<User> GetDistributionList(int userId);

    /// <summary>
    /// Get all product subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<UserProduct> GetUserProductSubscriptions(int userId);

    /// <summary>
    /// Get all report subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<UserReport> GetUserReportSubscriptions(int userId);

    /// <summary>
    /// Get all evening overview subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<UserAVOverview> GetUserEveningOverviewSubscriptions(int userId);

    /// <summary>
    /// Get all notification subscriptions for specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<UserNotification> GetUserNotificationSubscriptions(int userId);
}
