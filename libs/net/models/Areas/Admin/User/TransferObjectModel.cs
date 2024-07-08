namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// TransferObjectModel class, provides a model that represents an transfer account request.
/// </summary>
public class TransferObjectModel
{
    #region Properties
    public bool Checked { get; set; }
    public int OriginalId { get; set; }
    public string OriginalName { get; set; } = "";
    public int? NewId { get; set; }
    public string? NewName { get; set; }
    public bool SubscribeOnly { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TransferObjectModel.
    /// </summary>
    public TransferObjectModel() { }
    #endregion
}
