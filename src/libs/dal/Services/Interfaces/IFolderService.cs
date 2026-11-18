using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IFolderService : IBaseService<Folder, int>
{
    IEnumerable<Folder> FindMyFolders(int userId);
    IEnumerable<Folder> Find(FolderFilter? filter = null);
    Folder? FindById(int id, bool includeContent = false);
    IEnumerable<FolderContent> GetContentInFolder(int folderId);
    void RemoveContentFromFolders(long contentId);
    IEnumerable<Folder> GetFoldersWithFilters();

    void AddContentToFolder(long contentId, int folderId, bool toBottom = true);

    /// <summary>
    /// Clean content from the folder based on the folder's configuration settings.
    /// </summary>
    /// <param name="id"></param>
    void CleanFolder(int id);

    Folder UpdateAndSave(Folder entity, bool updateContent);
    Folder Update(Folder entity, bool updateContent);
}
