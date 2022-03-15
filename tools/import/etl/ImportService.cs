using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Tools.Import.Destination;
using TNO.Tools.Import.Destination.Entities;
using TNO.Tools.Import.Source;
using TNO.Tools.Import.Source.Entities;

namespace TNO.Tools.Import.ETL;

/// <summary>
/// ImportService class, provides a way to Extract, Transform, and Load data from the current Oracle TNO database and import it into the new PostgreSQL database.
/// </summary>
public class ImportService
{
    #region Variables
    private readonly ILogger _logger;
    private readonly SourceContext _sourceContext;
    private readonly DestinationContext _destinationContext;

    private List<ContentType> _contentTypes = new();
    private List<MediaType> _mediaTypes = new();
    private List<DataSource> _dataSources = new();
    private List<License> _licenses = new();
    private List<Category> _categories = new();
    private List<Destination.Entities.Action> _actions = new();
    private List<User> _users = new();
    private List<Series> _series = new();
    private List<Tag> _tags = new();
    private List<TonePool> _tonePools = new();

    private ContentType _defaultContentType = new("default");
    private User _defaultUser = new("default", "default", Guid.Empty);
    private License _defaultLicense = new("default", 0);
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ImportService object, initializes with specified parameters.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="sourceContext"></param>
    /// <param name="destinationContext"></param>
    public ImportService(ILogger<ImportService> logger, SourceContext sourceContext, DestinationContext destinationContext)
    {
        _logger = logger;
        _sourceContext = sourceContext;
        _destinationContext = destinationContext;
    }
    #endregion

    #region Methods
    private MediaType GetMediaType(string type)
    {
        var name = type;
        if (name == "Radio News") name = "News Radio";
        if (name == "TV News") name = "Television";

        return _mediaTypes.First(mt => mt.Name.ToLower() == name.ToLower());
    }

    private void InitializeLookups()
    {
        _contentTypes = _destinationContext.ContentTypes.ToList();
        _mediaTypes = _destinationContext.MediaTypes.ToList();
        _dataSources = _destinationContext.DataSources.ToList();
        _licenses = _destinationContext.Licenses.OrderBy(l => l.Id).ToList();
        _categories = _destinationContext.Categories.ToList();
        _series = _destinationContext.Series.ToList();
        _actions = _destinationContext.Actions.ToList();
        _tags = _destinationContext.Tags.ToList();
        _tonePools = _destinationContext.TonePools.ToList();
        _users = _destinationContext.Users.ToList();

        _defaultContentType = _contentTypes.First(ct => ct.Name == "Snippet");
        _defaultUser = _users.First();
        _defaultLicense = _licenses.First();
    }

    private string GetMimeType(string fileName, string contentType)
    {
        var ext = fileName.Split(".")[1];
        return ext switch
        {
            "mp4" => contentType,
            _ => contentType
        };
    }

    private Content AddContent(string uid, NewsItem newsItem)
    {
        var source = _dataSources.FirstOrDefault(ds => ds.Name.ToLower() == newsItem.Source.ToLower());
        var headline = String.IsNullOrWhiteSpace(newsItem.Title) ? "NO TITLE PROVIDED" : newsItem.Title;
        var content = new Content(uid, headline, newsItem.Source)
        {
            Status = newsItem.Published ? ContentStatus.Published : ContentStatus.Draft,
            WorkflowStatus = newsItem.Published ? WorkflowStatus.Published : WorkflowStatus.Success,
            DataSourceId = source?.Id,
            LicenseId = source?.LicenseId ?? _defaultLicense.Id,
            MediaTypeId = GetMediaType(newsItem.Type).Id,
            ContentTypeId = _defaultContentType.Id,
            OwnerId = _defaultUser.Id,
            Page = "",
            PublishedOn = newsItem.ItemDateTime.ToUniversalTime(),
            Summary = newsItem.Summary ?? "",
            Transcription = newsItem.Transcript ?? "",
            SourceUrl = newsItem.WebPath ?? ""
        };

        if (!String.IsNullOrWhiteSpace(newsItem.FileName)
          && !String.IsNullOrWhiteSpace(newsItem.FilePath)
          && !String.IsNullOrWhiteSpace(newsItem.ContentType))
        {
            var file = new FileReference(content, GetMimeType(newsItem.FileName, newsItem.ContentType), newsItem.FilePath);
            content.FileReferences.Add(file);
        }

        if (newsItem.FrontPageStory)
        {
            var action = _actions.First(a => a.Name == "Front Page");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.ThisJustIn)
        {
            var action = _actions.First(a => a.Name == "Just In");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.Commentary)
        {
            var action = _actions.First(a => a.Name == "Commentary");
            content.Actions.Add(new ContentAction(content, action, $"{newsItem.CommentaryTimeout}"));
        }
        if (newsItem.OnTicker)
        {
            var action = _actions.First(a => a.Name == "On Ticker");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.WapTopStory)
        {
            var action = _actions.First(a => a.Name == "Top Story");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.Alert)
        {
            var action = _actions.First(a => a.Name == "Alert");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (!String.IsNullOrWhiteSpace(newsItem.EodCategory))
        {
            var category = _categories.FirstOrDefault(a => a.Name.ToLower() == newsItem.EodCategory.ToLower());
            if (category != null)
            {
                content.Categories.Add(new ContentCategory(content, category, 0));
            }
            else
            {
                category = new Category(newsItem.EodCategory);
                _destinationContext.Add(category);
                _categories.Add(category);
                content.Categories.Add(new ContentCategory(content, category, 0));
            }
        }

        _destinationContext.Add(content);

        return content;
    }

    private Content UpdateContent(Content content, NewsItem newsItem)
    {
        content.Headline = String.IsNullOrWhiteSpace(newsItem.Title) ? "NO TITLE PROVIDED" : newsItem.Title;
        content.Status = newsItem.Published ? ContentStatus.Published : ContentStatus.Draft;
        content.WorkflowStatus = newsItem.Published ? WorkflowStatus.Published : WorkflowStatus.Success;
        content.PublishedOn = newsItem.ItemDateTime.ToUniversalTime();
        content.Summary = newsItem.Summary ?? "";
        content.Transcription = newsItem.Transcript ?? "";
        content.SourceUrl = newsItem.WebPath ?? "";

        if (!String.IsNullOrWhiteSpace(newsItem.FileName)
          && !String.IsNullOrWhiteSpace(newsItem.FilePath)
          && !String.IsNullOrWhiteSpace(newsItem.ContentType)
          && content.FileReferences.Count == 0)
        {
            var file = new FileReference(content, GetMimeType(newsItem.FileName, newsItem.ContentType), newsItem.FilePath);
            content.FileReferences.Add(file);
        }

        if (newsItem.FrontPageStory && !content.Actions.Any(a => a.Action?.Name == "Front Page"))
        {
            var action = _actions.First(a => a.Name == "Front Page");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.ThisJustIn && !content.Actions.Any(a => a.Action?.Name == "Just In"))
        {
            var action = _actions.First(a => a.Name == "Just In");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.Commentary && !content.Actions.Any(a => a.Action?.Name == "Commentary"))
        {
            var action = _actions.First(a => a.Name == "Commentary");
            content.Actions.Add(new ContentAction(content, action, $"{newsItem.CommentaryTimeout}"));
        }
        if (newsItem.OnTicker && !content.Actions.Any(a => a.Action?.Name == "On Ticker"))
        {
            var action = _actions.First(a => a.Name == "On Ticker");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.WapTopStory && !content.Actions.Any(a => a.Action?.Name == "Top Story"))
        {
            var action = _actions.First(a => a.Name == "Top Story");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (newsItem.Alert && !content.Actions.Any(a => a.Action?.Name == "Alert"))
        {
            var action = _actions.First(a => a.Name == "Alert");
            content.Actions.Add(new ContentAction(content, action, "true"));
        }
        if (!String.IsNullOrWhiteSpace(newsItem.EodCategory) && !content.Categories.Any(a => a.Category?.Name == newsItem.EodCategory))
        {
            var category = _categories.FirstOrDefault(a => a.Name.ToLower() == newsItem.EodCategory.ToLower());
            if (category != null)
            {
                content.Categories.Add(new ContentCategory(content, category, 0));
            }
            else
            {
                category = new Category(newsItem.EodCategory);
                _destinationContext.Add(category);
                _categories.Add(category);
                content.Categories.Add(new ContentCategory(content, category, 0));
            }
        }

        _destinationContext.Update(content);

        return content;
    }

    /// <summary>
    /// Run the import service.
    /// </summary>
    /// <param name="createdOn"></param>
    /// <returns></returns>
    public async Task<int> Run(DateTime createdOn)
    {
        _logger.LogInformation("Import Started");

        InitializeLookups();

        var skip = 0;
        var count = 20;

        while (count > 0)
        {
            try
            {
                var items = _sourceContext.NewsItems.Where(ni => ni.CreatedOn >= createdOn).OrderByDescending(ni => ni.UpdatedOn).OrderByDescending(ni => ni.RSN).Skip(skip).Take(count);
                count = items.Count();
                skip += count;

                await items.ForEachAsync(newsItem =>
                {
                    var uid = newsItem.WebPath ?? $"{newsItem.RSN}";

                    // Detect if content has already been imported.
                    var content = _destinationContext.Contents
                          .Include(c => c.Actions)
                          .Include(c => c.Categories)
                          .Include(c => c.DataSource)
                          .Include(c => c.FileReferences)
                          .Include(c => c.TonePools)
                          .Include(c => c.Tags)
                          .FirstOrDefault(c => c.Uid == uid);

                    if (content == null)
                    {
                        AddContent(uid, newsItem);
                        _logger.LogInformation("Adding content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
                    }
                    else
                    {
                        UpdateContent(content, newsItem);
                        _logger.LogInformation("Updating content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
                    }
                });

                _destinationContext.SaveChanges();
            }
            catch (Exception)
            {
                _logger.LogError("Import Failed on {skip}:{count}", skip, count);
                throw;
            }
        }

        _logger.LogInformation("Import Complete");
        return 0;
    }
    #endregion
}