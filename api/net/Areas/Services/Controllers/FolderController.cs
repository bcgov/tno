using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Folder;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// FolderController class, provides Folder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
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
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="folderService"></param>
    /// <param name="serializerOptions"></param>
    public FolderController(
        IFolderService folderService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _folderService = folderService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find folder for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult FindById(int id)
    {
        var result = _folderService.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new FolderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Get all folders with enabled filters.
    /// </summary>
    /// <returns></returns>
    [HttpGet("with-filters")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult GetFoldersWithFilters()
    {
        var results = _folderService.GetFoldersWithFilters();
        return new JsonResult(results.Select(result => new FolderModel(result, _serializerOptions)));
    }

    /// <summary>
    /// Removes the specified content from all folders.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpDelete("content/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult RemoveContentFromFolders(long contentId)
    {
        _folderService.RemoveContentFromFolders(contentId);
        return Ok();
    }

    /// <summary>
    /// Removes the specified content from all folders.
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="contentId"></param>
    /// <param name="bottom"></param>
    /// <returns></returns>
    [HttpPut("{folderId}/contents/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult AddContentToFolder(int folderId, long contentId, bool bottom = true)
    {
        _folderService.AddContentToFolder(contentId, folderId, bottom);
        return Ok();
    }

    /// <summary>
    /// Removes content from the folder based on the folder configuration settings.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id}/clean")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult RemoveContentFromFolder(int id)
    {
        _folderService.CleanFolder(id);
        return Ok();
    }
    #endregion
}
