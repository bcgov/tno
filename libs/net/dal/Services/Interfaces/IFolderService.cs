using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFolderService : IBaseService<Folder, int>
{
    IEnumerable<Folder> FindMyFolders(int userId);
    IEnumerable<Folder> FindAll();
}
