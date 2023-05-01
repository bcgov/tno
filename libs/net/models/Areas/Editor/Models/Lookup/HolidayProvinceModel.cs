namespace TNO.API.Areas.Editor.Models.Lookup;

/// <summary>
/// HolidayProvinceModel class, provides a model for Canadian holiday API.
/// </summary>
public class HolidayProvinceModel
{
    #region Properties
    public string Id { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string NameFr { get; set; } = "";
    public Uri? SourceLink { get; set; }
    public string SourceEn { get; set; } = "";
    public HolidayModel? NextHoliday { get; set; }
    public IEnumerable<HolidayModel> Holidays { get; set; } = Array.Empty<HolidayModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HolidayProvinceModel.
    /// </summary>
    public HolidayProvinceModel() { }
    #endregion
}
