using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewInstanceService : IBaseService<AVOverviewInstance, int>
{
    /// <summary>
    /// Return the first evening overview instance for the specified date.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    AVOverviewInstance? FindByDate(DateTime publishedOn);
}
