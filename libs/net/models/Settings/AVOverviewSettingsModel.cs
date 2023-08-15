using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class AVOverviewSettingsModel
{
    #region Properties
    #endregion

    #region Constructors
    public AVOverviewSettingsModel() { }

    public AVOverviewSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        // this.ViewOnWebOnly = settings.GetDictionaryJsonValue("viewOnWebOnly", false, options);
    }
    #endregion
}
