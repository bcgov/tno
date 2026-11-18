namespace TNO.API.Areas.Editor.Models.Lookup;

/// <summary>
/// CanadaHolidayModel class, provides a model for Canadian holiday API.
/// </summary>
public class CanadaHolidayModel
{
    #region Properties
    public HolidayProvinceModel? Province { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CanadaHolidayModel.
    /// </summary>
    public CanadaHolidayModel() { }
    #endregion
}
