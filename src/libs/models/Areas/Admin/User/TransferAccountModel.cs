namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// TransferAccountModel class, provides a model that represents an transfer account request.
/// </summary>
public class TransferAccountModel
{
    #region Properties
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public bool TransferOwnership { get; set; }
    public bool IncludeHistory { get; set; }
    public bool IsReady { get; set; }
    public TransferNotificationModel[] Notifications { get; set; } = Array.Empty<TransferNotificationModel>();
    public TransferFilterModel[] Filters { get; set; } = Array.Empty<TransferFilterModel>();
    public TransferFolderModel[] Folders { get; set; } = Array.Empty<TransferFolderModel>();
    public TransferReportModel[] Reports { get; set; } = Array.Empty<TransferReportModel>();
    public TransferProductModel[] Products { get; set; } = Array.Empty<TransferProductModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TransferAccountModel.
    /// </summary>
    public TransferAccountModel() { }
    #endregion
}
