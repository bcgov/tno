using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Folder;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// FolderController class, provides folder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly IUserService _userService;
    private readonly IImpersonationHelper _impersonate;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="folderService"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="userService"></param>
    /// <param name="impersonateHelper"></param>
    public FolderController(
        IFolderService folderService,
        IUserService userService,
        IImpersonationHelper impersonateHelper,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _folderService = folderService;
        _serializerOptions = serializerOptions.Value;
        _userService = userService;
        _impersonate = impersonateHelper;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find folder for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult FindById(int id)
    {
        var user = _impersonate.GetCurrentUser();
        var folder = _folderService.FindById(id) ?? throw new NoContentException();
        if (folder.OwnerId != user.Id) throw new NotAuthorizedException();
        return new JsonResult(new FolderModel(folder, _serializerOptions));
    }


    /// <summary>
    /// Find all "my" folders.
    /// </summary>
    /// <returns></returns>
    [HttpGet("my-folders")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<FolderModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult FindMyFolders()
    {
        var user = _impersonate.GetCurrentUser();
        return new JsonResult(_folderService.FindMyFolders(user.Id).Select(ds => new FolderModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Add folder for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult Add([FromBody] FolderModel model)
    {
        var user = _impersonate.GetCurrentUser();
        model.OwnerId = user.Id;
        var result = _folderService.AddAndSave(model.ToEntity(_serializerOptions));
        var folder = _folderService.FindById(result.Id) ?? throw new NoContentException("Folder does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new FolderModel(folder, _serializerOptions));
    }

    /// <summary>
    /// Update folder for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateContent"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult Update([FromBody] FolderModel model, bool updateContent = false)
    {
        var user = _impersonate.GetCurrentUser();
        var folder = _folderService.FindById(model.Id) ?? throw new NoContentException("Folder does not exist");
        if (folder.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to update folder");
        _folderService.ClearChangeTracker();
        var result = _folderService.UpdateAndSave(model.ToEntity(_serializerOptions), updateContent);
        folder = _folderService.FindById(result.Id) ?? throw new NoContentException("Folder does not exist");
        return new JsonResult(new FolderModel(folder, _serializerOptions));
    }

    /// <summary>
    /// Delete folder for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Folder" })]
    public IActionResult Delete([FromBody] FolderModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var folder = _folderService.FindById(model.Id) ?? throw new NoContentException("Folder does not exist");
        if (folder.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to delete folder");
        _folderService.DeleteAndSave(folder);
        return new JsonResult(model);
    }
    #endregion
}
