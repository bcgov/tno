namespace TNO.Services.Filemonitor;

/// <summary>
/// Field names/values for the extraction of newspaper data.
/// </summary>
public static class Fields
{
    /// <summary>
    /// Key to the "papername" connection string value.
    /// </summary>
    public const string Papername = "papername";

    /// <summary>
    /// Key to the "headline" connection string value.
    /// </summary>
    public const string Headline = "headline";

    /// <summary>
    /// Key to the "summary" connection string value.
    /// </summary>
    public const string Summary = "summary";

    /// <summary>
    /// Key to the "story" connection string value.
    /// </summary>
    public const string Story = "story";

    /// <summary>
    /// Key to the "author" connection string value.
    /// </summary>
    public const string Author = "author";

    /// <summary>
    /// Key to the "date" connection string value.
    /// </summary>
    public const string Date = "date";

    /// <summary>
    /// Key to the "lang" connection string value.
    /// </summary>
    public const string Lang = "lang";

    /// <summary>
    /// Key to the "section" connection string value.
    /// </summary>
    public const string Section = "section";

    /// <summary>
    /// Key to the "id" connection string value.
    /// </summary>
    public const string Id = "id";

    /// <summary>
    /// Key to the "tags" connection string value.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Key to the "page" connection string value.
    /// </summary>
    public const string Page = "page";

    /// <summary>
    /// Key to the "item" connection string value.
    /// </summary>
    public const string Item = "item";

    /// <summary>
    /// Key to the "dateFmt" connection string value.
    /// </summary>
    public const string DateFmt = "dateFmt";

    /// <summary>
    /// Key to the "escapeContent" connection string value.
    /// </summary>
    public const string EscapeContent = "escapeContent";

    /// <summary>
    /// Key to the "addParent" connection string value.
    /// </summary>
    public const string AddParent = "addParent";

    /// <summary>
    /// Key to the "fileFormat" connection string value.
    /// </summary>
    public const string FileFormat = "fileFormat";

    /// <summary>
    /// Key to the "importDir" connection string value.
    /// </summary>
    public const string ImportDir = "importDir";

    /// <summary>
    /// Key to the "selfPublished" connection string value.
    /// </summary>
    public const string SelfPublished = "selfPublished";

    /// <summary>
    /// FMS story delimiter string.
    /// </summary>
    public const string FmsStoryDelim = "</story><story>";

    /// <summary>
    /// FMS story delimiter string.
    /// </summary>
    public const string FmsEofFlag = "</story>";

    /// <summary>
    /// FMS field delimiter string.
    /// </summary>
    public const string FmsFieldDelim = "<break>";
}
