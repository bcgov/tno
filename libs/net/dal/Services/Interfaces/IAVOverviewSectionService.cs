using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewSectionService : IBaseService<AVOverviewSection, int>
{
    IEnumerable<AVOverviewSection> FindAll();
}
