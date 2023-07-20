using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAVOverviewSectionItemService : IBaseService<AVOverviewSectionItem, int>
{
    IEnumerable<AVOverviewSectionItem> FindAll();
    IEnumerable<AVOverviewSectionItem> FindBySectionId(int sectionId);

}
