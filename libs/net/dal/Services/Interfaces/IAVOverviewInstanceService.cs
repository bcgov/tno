using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewInstanceService : IBaseService<AVOverviewInstance, long>
{
    /// <summary>
    /// Return the first evening overview instance for the specified date.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    AVOverviewInstance? FindByDate(DateTime publishedOn);

    /// <summary>
    /// Return the latest published evening overview instance.
    /// </summary>
    /// <returns></returns>
    AVOverviewInstance? FindLatest();

    IEnumerable<UserAVOverviewInstance> GetUserAVOverviewInstances(long instanceId);
    UserAVOverviewInstance Add(UserAVOverviewInstance entity);
    UserAVOverviewInstance AddAndSave(UserAVOverviewInstance entity);
    UserAVOverviewInstance Update(UserAVOverviewInstance entity);
    UserAVOverviewInstance UpdateAndSave(UserAVOverviewInstance entity);
    IEnumerable<UserAVOverviewInstance> UpdateAndSave(IEnumerable<UserAVOverviewInstance> entities);
    UserAVOverviewInstance Delete(UserAVOverviewInstance entity);
    UserAVOverviewInstance DeleteAndSave(UserAVOverviewInstance entity);
}
