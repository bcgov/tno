using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Folder;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// FolderController class, provides folder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/folders")]
[Route("api/[area]/folders")]
[Route("v{version:apiVersion}/[area]/folders")]
[Route("[area]/folders")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class FolderController : ControllerBase
{
    #region Variables
    private readonly IFolderService _folderService;
    private readonly ITopicScoreService _topicScoreService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="folderService"></param>
    /// <param name="topicScoreService"></param>
    /// <param name="serializerOptions"></param>
    public FolderController(
        IFolderService folderService,
        ITopicScoreService topicScoreService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _folderService = folderService;
        _topicScoreService = topicScoreService;

        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Get folder content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeCalculatedTopicScore">return the calculated topic score for each piece of content in the folder</param>
    /// <returns></returns>
    [HttpGet("{id}/content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<FolderContentModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult GetContent(int id, bool includeCalculatedTopicScore = false)
    {
        var result = _folderService.GetContentInFolder(id) ?? throw new NoContentException();

        if (includeCalculatedTopicScore)
        {
            // The score the topic score rules give each story, which caps the score an editor can choose.
            var scores = _topicScoreService.CalculateScores(result.Where(f => f.Content != null).Select(f => f.ContentId));
            return new JsonResult(result.Select(f => new FolderContentModel(f)
            {
                CalculatedTopicScore = scores.TryGetValue(f.ContentId, out var score) ? score : 0
            }));
        }
        else
            return new JsonResult(result.Select(f => new FolderContentModel(f)));
    }
    #endregion
}
