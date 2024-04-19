using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models;
using TNO.API.Areas.Subscriber.Models.Content;
using TNO.API.Areas.Subscriber.Models.Storage;
using TNO.API.Config;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Extensions;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/contents")]
[Route("api/[area]/contents")]
[Route("v{version:apiVersion}/[area]/contents")]
[Route("[area]/contents")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IFileReferenceService _fileReferenceService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly StorageOptions _storageOptions;
    private readonly ElasticOptions _elasticOptions;
    private readonly INotificationService _notificationService;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="notificationService"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<ElasticOptions> elasticOptions,
        INotificationService notificationService,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _storageOptions = storageOptions.Value;
        _elasticOptions = elasticOptions.Value;
        _notificationService = notificationService;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified Elasticsearch query filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="includeUnpublishedContent"></param>
    /// <returns></returns>
    [HttpPost("search")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> FindWithElasticsearchAsync([FromBody] JsonDocument filter, [FromQuery] bool includeUnpublishedContent = false)
    {
        // Exclude any content the user is not allowed to search for.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("User is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        user = _userService.FindById(user.Id) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        filter = filter.AddExcludeSources(user.SourcesManyToMany.Select(s => s.SourceId));
        filter = filter.AddExcludeMediaTypes(user.MediaTypesManyToMany.Select(s => s.MediaTypeId));

        var result = await _contentService.FindWithElasticsearchAsync(includeUnpublishedContent ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex, filter);
        return new JsonResult(result);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindById(long id)
    {
        var result = _contentService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new ContentModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id' and download the file it references.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult DownloadFile(long id)
    {
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault() ?? throw new NoContentException("File does not exist");
        var stream = _fileReferenceService.Download(fileReference, _storageOptions.GetUploadPath());
        return File(stream, fileReference.ContentType);
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Stream([FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
        if (!safePath.FileExists()) throw new NoContentException("File does not exist");

        var info = new ItemModel(safePath);
        var fileStream = System.IO.File.OpenRead(safePath);
        return File(fileStream, info.MimeType!);
    }

    /// <summary>
    /// Add the new content to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Add(ContentModel model)
    {
        // Always make the user who created the content the owner.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var newContent = (Entities.Content)model;
        newContent.OwnerId = user.Id;
        newContent.PostedOn = DateTime.UtcNow;
        var content = _contentService.AddAndSave(newContent);

        return CreatedAtAction(nameof(FindById), new { id = content.Id }, new ContentModel(content));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// Only allow users to update their own content, or to add/update their own version of the content.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateAsync(ContentModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var content = _contentService.FindById(model.Id) ?? throw new NoContentException("Content does not exist");

        // If user does not own the content and hasn't submitted a version.
        if (content.OwnerId != user.Id && !model.Versions.ContainsKey(user.Id)) throw new NotAuthorizedException("User does not own content");
        _contentService.ClearChangeTracker();

        if (content.OwnerId == user.Id)
        {
            // If user owns the content they can update it.
            content.Uid = model.Uid;
            content.SourceUrl = model.SourceUrl;
            content.PublishedOn = model.PublishedOn;
            content.Headline = model.Headline;
            content.Byline = model.Byline;
            content.OtherSource = model.OtherSource;
            content.Section = model.Section;
            content.Edition = model.Edition;
            content.Page = model.Page;
            content.Summary = model.Summary;
            content.Body = model.Body;
            content.Versions = model.Versions;
            foreach (var tonePool in model.TonePools)
            {
                // Update or Add.
                var currentTonePool = content.TonePoolsManyToMany.FirstOrDefault((ctp) => ctp.TonePoolId == tonePool.Id);
                if (currentTonePool == null)
                    content.TonePoolsManyToMany.Add(new Entities.ContentTonePool(tonePool.ContentId, tonePool.Id, tonePool.Value));
                else if (currentTonePool.Value != tonePool.Value)
                    currentTonePool.Value = tonePool.Value;
            }
            content.Version = model.Version ?? 0;
            content = _contentService.UpdateAndSave(content);
        }
        else
        {
            if (content.Versions.ContainsKey(user.Id))
            {
                // Update the version.
                content.Versions[user.Id] = model.Versions[user.Id];
            }
            else
            {
                // Add version.
                content.Versions.Add(user.Id, model.Versions[user.Id]);
            }

            content = _contentService.UpdateAndSave(content);
        }

        // Inform other users of the updated content.
        if (!content.IsPrivate)
            await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content) })));

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Delete(ContentModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var content = _contentService.FindById(model.Id) ?? throw new NoContentException("Content does not exist");

        // If user does not own the content and hasn't submitted a version.
        if (content.OwnerId != user.Id && !model.Versions.ContainsKey(user.Id)) throw new NotAuthorizedException("User does not own content");

        if (content.OwnerId == user.Id)
        {
            // Delete custom content owned by user.
            _contentService.DeleteAndSave(content);
        }
        else
        {
            // Remove the version only.
            content.Versions.Remove(user.Id);
            _contentService.UpdateAndSave(content);
        }
        return new JsonResult(model);
    }

    /// <summary>
    /// Send the notification to the specified colleague.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="colleagueId"></param>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    [HttpPost("{contentId}/share")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public async Task<IActionResult> ShareAsync(long contentId, int colleagueId, int notificationId)
    {
        var notification = _notificationService.FindById(notificationId) ?? throw new NoContentException();

        var username = "admin"; // User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        var colleague = _userService.FindById(colleagueId) ?? throw new NotAuthorizedException("Colleague does not exist");

        var request = new NotificationRequestModel(NotificationDestination.NotificationService, new { })
        {
            NotificationId = notification.Id,
            ContentId = contentId,
            RequestorId = user.Id,
            To = colleague.PreferredEmail != "" ? colleague.PreferredEmail : colleague.Email,
            IsPreview = true,
            IgnoreValidation = true,
        };
        await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NotificationTopic, request);
        return new JsonResult(new NotificationModel(notification, _serializerOptions));
    }

    /// <summary>
    /// Send the notification to the specified email address.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="email"></param>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    [HttpPost("{contentId}/share/email")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public async Task<IActionResult> ShareEmailAsync(long contentId, string email, int notificationId)
    {
        var notification = _notificationService.FindById(notificationId) ?? throw new NoContentException();

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var subscriber = _userService.FindByEmail(email).FirstOrDefault()  ?? throw new InvalidOperationException("Subscriber does not exist");

        var request = new NotificationRequestModel(NotificationDestination.NotificationService, new { })
        {
            NotificationId = notification.Id,
            ContentId = contentId,
            RequestorId = user.Id,
            To = subscriber.PreferredEmail != "" ? subscriber.PreferredEmail : subscriber.Email,
            IsPreview = true,
            IgnoreValidation = true,
        };
        await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NotificationTopic, request);
        return new JsonResult(new NotificationModel(notification, _serializerOptions));
    }
    #endregion
}
