namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// TransferReportSectionModel class, provides a model that represents an transfer account request.
/// </summary>
public class TransferReportSectionModel : TransferObjectModel
{
    #region Properties
    public long FilterId { get; set; }
    public long FolderId { get; set; }
    public long LinkedReportId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TransferReportSectionModel.
    /// </summary>
    public TransferReportSectionModel() { }
    #endregion
}
