using System.Net;
using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Extensions;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
/// manages Ingestion of TNO 1.0 'Paper'
/// </summary>
public class PaperMigrator : ContentMigrator<ContentMigrationOptions>, IContentMigrator
{

    /// <summary>
    ///
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public PaperMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrator<ContentMigrationOptions>> logger) : base(api, migratorOptions, options, logger)
    {
    }

    /// <summary>
    /// Creates an Paper SourceContent from a NewsItem
    /// </summary>
    /// <param name="lookups"></param>
    /// <param name="source"></param>
    /// <param name="product"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <returns></returns>
    public override SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, ProductModel product, ContentType contentType, NewsItem newsItem)
    {
        // var authors = GetAuthors(lookups.Contributors)
        DateTime publishedOn = newsItem.GetPublishedDateTime().ToUniversalTime();
        var newsItemTitle = newsItem.GetTitle();

        var content = new SourceContent(
            this.Options.DataLocation,
            source.Code,
            contentType,
            product.Id,
            GetContentHash(source.Code, newsItemTitle, publishedOn),
            newsItemTitle,
            newsItem.Summary! ?? string.Empty,
            GetNewsItemBody(newsItem.Text, newsItem.Summary),
            publishedOn,
            newsItem.Published)
        {
            Section = newsItem.string2 ?? string.Empty,
            Page = newsItem.string3 ?? string.Empty,
            FilePath = newsItem.FilePath ?? string.Empty,
            Link = newsItem.WebPath ?? string.Empty,
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            ExternalUid = newsItem.WebPath ?? string.Empty
        };

        if (string.IsNullOrEmpty(this.Options.DefaultUserNameForAudit)) throw new System.Configuration.ConfigurationErrorsException("Default Username for ContentMigration has not been configured");
        var auditUser = lookups.Users.FirstOrDefault(u => u.Username == this.Options.DefaultUserNameForAudit);
        if (auditUser == null) throw new System.Configuration.ConfigurationErrorsException($"Default User for ContentMigration not found : {this.Options.DefaultUserNameForAudit}");

        if (newsItem.Tones?.Any() == true)
        {
            // TODO: replace the USER_RSN value on UserIdentifier with something that can be mapped by the Content Service to an MMIA user
            // TODO: remove UserRSN filter once user can be mapped
            content.TonePools = newsItem.Tones.Where(t => t.UserRSN == 0)
                .Select(t => new Kafka.Models.TonePool((int)t.ToneValue, t.UserRSN.ToString()));
        }

        // Extract authors from a "delimited" string.  Don't use the source name as an author.
        if (!string.IsNullOrEmpty(newsItem.string5) || !string.IsNullOrEmpty(newsItem.string6))
        {
            var authors = new List<string>();
            if (!string.IsNullOrEmpty(newsItem.string5)) authors.AddRange(ExtractAuthors(newsItem.string5, newsItem.Source));
            if (!string.IsNullOrEmpty(newsItem.string6)) authors.AddRange(ExtractAuthors(newsItem.string6, newsItem.Source));
            content.Authors = authors.Distinct().Select(a => new Author(a));
        }

        if (newsItem.UpdatedOn != null)
        {
            content.UpdatedOn = newsItem.UpdatedOn != DateTime.MinValue ? newsItem.UpdatedOn.Value.ToUniversalTime() : null;
        }

        if (!string.IsNullOrEmpty(newsItem.EodGroup) && !string.IsNullOrEmpty(newsItem.EodCategory))
        {
            content.Topics = new[] { new Kafka.Models.Topic { Name = newsItem.EodCategory, TopicType = (TopicType)Enum.Parse(typeof(TopicType), newsItem.EodGroup) } };
        }

        // Tags are in the Summary as they are added by an Editor
        if (!string.IsNullOrEmpty(newsItem.Summary))
        {
            // if Tags are found, let the ContentManagement service decide if they are new or not
            content.Tags = ExtractTags(newsItem.Summary)
                .Select(c => new TNO.Kafka.Models.Tag(c.ToUpperInvariant(), ""));
        }

        // map relevant news item properties to actions
        content.Actions = GetActionMappings(newsItem.FrontPageStory, newsItem.WapTopStory, newsItem.Alert,
            newsItem.Commentary, newsItem.CommentaryTimeout);

        // the total "Effort" is stored in the Number2 field as seconds
        if (newsItem.Number2.HasValue && newsItem.Number2 > 0)
        {
            content.TimeTrackings = new[] { new Kafka.Models.TimeTrackingModel {
                Activity = this.Options.DefaultTimeTrackingActivity,
                Effort = (float)Math.Round(Convert.ToDecimal(newsItem.Number2) / 60, 2),
                UserId = auditUser.Id }};
        }

        return content;
    }

    /// <summary>
    /// fix the replace of paragraph markers with "|"
    /// </summary>
    /// <param name="text"></param>
    /// <param name="summary"></param>
    /// <returns>formatted body string</returns>
    private string GetNewsItemBody(string text, string summary)
    {
        string body = string.Empty;

        if (!string.IsNullOrEmpty(text))
        {
            string const marker = '|';
            int index = text.IndexOf(marker);
            if (index !== -1)
            {
                // found at least one linebreak marker
                body = $"<p>{text.Replace(marker, "</p><p>")}</p>";
            }
            else
            {
                // no line breaks apparently
                body = text;
            }
        }
        else if (!string.IsNullOrEmpty(summary))
        {
            body = summary;
        }
        return body;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override System.Linq.Expressions.Expression<Func<NewsItem, bool>> GetBaseFilter()
    {
        return PredicateBuilder.New<NewsItem>()
                               .And(ni => !ni.ContentType!.Equals("video/quicktime"))
                               .And(ni => !ni.ContentType!.Equals("image/jpeg"));
    }
}
