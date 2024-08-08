using System.Text.RegularExpressions;

namespace TNO.Elastic;

/// <summary>
/// StringExtensions static class, provides extensions methods for string objects.
/// </summary>
public static partial class RegexSettings
{
    #region Variables

    [GeneratedRegex(":([\\[\\{])")]
    public static partial Regex RangeRegex();

    [GeneratedRegex("[\\^\\~][0-9]*$")]
    public static partial Regex BoostOrProximityRegex();

    [GeneratedRegex("[\\*\\?]")]
    public static partial Regex WildCardRegex();
    #endregion
}
