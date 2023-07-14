using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFolderService : IBaseService<Folder, int>
{
    IEnumerable<Folder> FindAll();
}
