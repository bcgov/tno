namespace TNO.Services.FileMonitor;

/// <summary>
/// Field names/values for the extraction of newspaper data.
/// </summary>
public static class Fields
{
    /// <summary>
    /// Key to the "papername" configuration string value.
    /// </summary>
    public const string PaperName = "papername";

    /// <summary>
    /// Key to the "headline" configuration string value.
    /// </summary>
    public const string Headline = "headline";

    /// <summary>
    /// Key to the "summary" configuration string value.
    /// </summary>
    public const string Summary = "summary";

    /// <summary>
    /// Key to the "story" configuration string value.
    /// </summary>
    public const string Story = "story";

    /// <summary>
    /// Key to the "author" configuration string value.
    /// </summary>
    public const string Author = "author";

    /// <summary>
    /// Key to the "date" configuration string value.
    /// </summary>
    public const string Date = "date";

    /// <summary>
    /// Key to the "lang" configuration string value.
    /// </summary>
    public const string Lang = "lang";

    /// <summary>
    /// Key to the "section" configuration string value.
    /// </summary>
    public const string Section = "section";

    /// <summary>
    /// Key to the "id" configuration string value.
    /// </summary>
    public const string Id = "id";

    /// <summary>
    /// Key to the "tags" configuration string value.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Key to the "page" configuration string value.
    /// </summary>
    public const string Page = "page";

    /// <summary>
    /// Key to the "item" configuration string value.
    /// </summary>
    public const string Item = "item";

    /// <summary>
    /// Key to the "dateFmt" configuration string value.
    /// </summary>
    public const string DateFmt = "dateFmt";

    /// <summary>
    /// Key to the "escapeContent" configuration string value.
    /// </summary>
    public const string EscapeContent = "escapeContent";

    /// <summary>
    /// Key to the "addParent" configuration string value.
    /// </summary>
    public const string AddParent = "addParent";

    /// <summary>
    /// Key to the "fileFormat" configuration string value.
    /// </summary>
    public const string FileFormat = "fileFormat";

    /// <summary>
    /// Key to the "importDir" configuration string value.
    /// </summary>
    public const string ImportDir = "importDir";

    /// <summary>
    /// Key to the "sources" configuration string value.
    /// </summary>
    public const string Sources = "sources";

    /// <summary>
    /// FMS story delimiter string.
    /// </summary>
    public const string FmsEntryStart = "<story>";

    /// <summary>
    /// FMS story delimiter string.
    /// </summary>
    public const string FmsEntryEnd = "</story>";

    /// <summary>
    /// Whether to fix the Blacks XML issue.
    /// </summary>
    public const string FixBlacksXml = "fixBlacksXml";
}
