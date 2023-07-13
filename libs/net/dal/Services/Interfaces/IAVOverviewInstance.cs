using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewInstanceService : IBaseService<AVOverviewInstance, int>
{
    IEnumerable<AVOverviewInstance> FindAll();
}
