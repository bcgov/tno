namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// TransferReportModel class, provides a model that represents an transfer account request.
/// </summary>
public class TransferReportModel : TransferObjectModel
{
    #region Properties
    public TransferReportSectionModel[] Sections { get; set; } = Array.Empty<TransferReportSectionModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TransferReportModel.
    /// </summary>
    public TransferReportModel() { }
    #endregion
}
