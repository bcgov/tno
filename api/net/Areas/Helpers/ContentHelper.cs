
using System.Web;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Services;

namespace TNO.API.Helpers;

/// <summary>
/// ContentHelper class, provides helper methods for content.
/// </summary>
public class ContentHelper : IContentHelper
{
    #region Variables
    private readonly IFileReferenceService _fileReferenceService;
    private readonly StorageOptions _storageOptions;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="fileReferenceService"></param>
    /// <param name="storageOptions"></param>
    public ContentHelper(
        IFileReferenceService fileReferenceService,
        IOptions<StorageOptions> storageOptions)
    {
        _fileReferenceService = fileReferenceService;
        _storageOptions = storageOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate a base 64 string for the image associated with the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public async Task<string?> GetImageAsync(long contentId)
    {
        var fileReference = _fileReferenceService.FindByContentId(contentId).FirstOrDefault();
        if (fileReference == null) return null;

        var safePath = Path.Combine(
            _storageOptions.GetUploadPath(),
            HttpUtility.UrlDecode(fileReference.Path).MakeRelativePath());
        if (!safePath.FileExists()) return null;

        using var fileStream = new FileStream(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        await fileStream.ReadExactlyAsync(imageBytes.AsMemory(0, (int)fileStream.Length));
        return Convert.ToBase64String(imageBytes);
    }
    #endregion
}
