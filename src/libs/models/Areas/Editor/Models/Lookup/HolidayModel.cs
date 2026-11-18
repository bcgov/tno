using System.Text.Json.Serialization;
using TNO.Core.Converters;

namespace TNO.API.Areas.Editor.Models.Lookup;

/// <summary>
/// HolidayModel class, provides a model for Canadian holiday API.
/// </summary>
public class HolidayModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key of the holiday.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The date of the holiday.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// get/set - The English name of the holiday.
    /// </summary>
    public string NameEn { get; set; } = "";

    /// <summary>
    /// get/set - The French name of the holiday.
    /// </summary>
    public string NameFr { get; set; } = "";

    /// <summary>
    /// get/set - Whether this is a federal stat holiday.
    /// </summary>
    [JsonConverter(typeof(BooleanConverter))]
    public bool Federal { get; set; }

    /// <summary>
    /// get/set - The date the holiday is observed.
    /// </summary>
    public DateTime ObservedDate { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HolidayModel.
    /// </summary>
    public HolidayModel() { }
    #endregion
}
