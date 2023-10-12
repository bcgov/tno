using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFolderService : IBaseService<Folder, int>
{
    IEnumerable<Folder> FindMyFolders(int userId);
    IEnumerable<Folder> FindAll();
    IEnumerable<FolderContent> GetContentInFolder(int folderId);
    void RemoveContentFromFolders(long contentId);
    IEnumerable<Folder> GetFoldersWithFilters();

    void AddContentToFolder(long contentId, int folderId, bool toBottom = true);
}
