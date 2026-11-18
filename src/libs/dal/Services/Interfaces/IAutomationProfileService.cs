using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAutomationProfileService : IBaseService<AutomationProfile, int>
{
    /// <summary>
    /// Find all automation profiles with their steps and actions.
    /// </summary>
    /// <returns></returns>
    IEnumerable<AutomationProfile> FindAll();
}
