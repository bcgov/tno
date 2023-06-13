using TNO.Services.Config;

namespace TNO.Services.ContentMigration.Config;

/// <summary>
/// ContentMigrationOptions class, configuration options for Import
/// </summary>
public class PaperMigrationOptions : IngestServiceOptions
{
    #region Properties

    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Source' field value
    /// doesn't match the Code or Name of any MMIA Source
    /// The Key should be the "TNO 1.0 Source"
    /// The Value should be the "MMIA Source Code"
    /// </summary>
    public Dictionary<string, string> IngestSourceMappings { get; set; } = new Dictionary<string, string>()
        {
           { "BiV", "BIV" },
            { "CBC | Aboriginal News", "CBCINDIGNEWS" },
            { "CJVB Online", "CJVB" },
            //{ "Canadian Evergreen", ""},
            //{ "Casino Reports", ""},
            {"Castanet.net - Most Recent Stories", "CASTANET"},
            //{ "Comox Valley Record", "" },
            { "CP News", "CPNEWS" },
            { "Eagle Valley News", "SEVN" },
            //{ "East Kootenay News Online Weekly", "" },
            //{ "Gambling Insider", "" },
            //{ "Global News Okangan RSS", "" },
            //{ "Global News RSS","" },
            //{ "Journal of Commerce", "" },
            //{ "My East Kootenay Now", "" },
            //{ "Northern Beat", "" },
            //{ "Northern Beat News", "" },
            //{ "Penticton Herald", ""}
            //{ "Powell River Peak", "" },
            //{ "Salt Spring Exchange", ""}
            { "The Hook Home", "TYEE" }, // KGM: was a blog under the TYEE banner
            //{ "The Squamish Chief", "" },
            //{ "Todayville", "" },
            //{ "West Coast Traveller", "" },
            //{ "West K News", "" },
            //{ "Yogonet", "" },
        };

    /// <summary>
    /// Stores a mapping to be used where the TNO 1.0 database 'Type' field value
    /// doesn't match the Code or Name of any MMIA Source
    /// The Key should be the "TNO 1.0 Type"
    /// The Value should be the "MMIA Product Name"
    /// </summary>
    public Dictionary<string, string> ProductMappings { get; set; } = new Dictionary<string, string>()
        {
            { "Newspaper", "Daily Print" },
            { "CP News", "CP Wire" },
            { "Regional", "Weekly Print" },
            { "Internet", "Online" }
        };

    #endregion
}
