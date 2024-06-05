namespace TNO.Services.ContentMigration.Sources.Oracle;
/// <summary>
/// HNewsItem class, provides an entity to store Historical News Item records in the database.
/// </summary>
public class HNewsItem : BaseNewsItem
{

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="item"></param>
    public static explicit operator NewsItem(HNewsItem item)
    {
        var newsItem = new NewsItem()
        {
            RSN = item.RSN,
            ItemDate = item.ItemDate,
            ItemTime = item.ItemTime,
            Source = item.Source,
            Summary = item.Summary,
            Title = item.Title,
            Type = item.Type,
            FrontPageStory = item.FrontPageStory,
            Published = item.Published,
            Archived = item.Archived,
            ArchivedTo = item.ArchivedTo,
            CreatedOn = item.CreatedOn,
            UpdatedOn = item.UpdatedOn,
            string1 = item.string1,
            string2 = item.string2,
            string3 = item.string3,
            string4 = item.string4,
            string5 = item.string5,
            string6 = item.string6,
            string7 = item.string7,
            string8 = item.string8,
            string9 = item.string9,
            Number1 = item.Number1,
            Number2 = item.Number2,
            Date1 = item.Date1,
            Date2 = item.Date2,
            FileName = item.FileName,
            FilePath = item.FilePath,
            WebPath = item.WebPath,
            ThisJustIn = item.ThisJustIn,
            ImportedFrom = item.ImportedFrom,
            ExpireRule = item.ExpireRule,
            Commentary = item.Commentary,
            Text = item.Text,
            Binary = item.Binary,
            ContentType = item.ContentType,
            BinaryLoaded = item.BinaryLoaded,
            BinaryLoad = item.BinaryLoad,
            BinaryExternal = item.BinaryExternal,
            Cbra = item.Cbra,
            PostedBy = item.PostedBy,
            OnTicker = item.OnTicker,
            WapTopStory = item.WapTopStory,
            Alert = item.Alert,
            AutoTone = item.AutoTone,
            CategoriesLocked = item.CategoriesLocked,
            CoreAlert = item.CoreAlert,
            CommentaryTimeout = item.CommentaryTimeout,
            CommentaryExpireTime = item.CommentaryExpireTime,
            Transcript = item.Transcript,
            EodCategory = item.EodCategory,
            EodGroup = item.EodGroup,
            EodDateTime = item.EodDateTime,
            Tones = item.Tones,
            Topics = item.Topics,
        };

        return newsItem;
    }
}
