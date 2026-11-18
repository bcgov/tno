namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// TransferFolderModel class, provides a model that represents an transfer account request.
/// </summary>
public class TransferFolderModel : TransferObjectModel
{
    #region Properties
    public long FilterId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TransferFolderModel.
    /// </summary>
    public TransferFolderModel() { }
    #endregion
}
