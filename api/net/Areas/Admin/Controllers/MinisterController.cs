using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Minister;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// MinisterController class, provides minister endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/ministers")]
[Route("api/[area]/ministers")]
[Route("v{version:apiVersion}/[area]/ministers")]
[Route("[area]/ministers")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class MinisterController : ControllerBase
{
    #region Variables
    private readonly IMinisterService _ministerService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MinisterController object, initializes with specified parameters.
    /// </summary>
    /// <param name="ministerService"></param>
    public MinisterController(IMinisterService ministerService)
    {
        _ministerService = ministerService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all ministers.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<MinisterModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Minister" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_ministerService.FindAll().Select(ds => new MinisterModel(ds)));
    }

    /// <summary>
    /// Find minister for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MinisterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Minister" })]
    public IActionResult FindById(int id)
    {
        var result = _ministerService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new MinisterModel(result));
    }

    /// <summary>
    /// Add minister for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MinisterModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Minister" })]
    public IActionResult Add([FromBody] MinisterModel model)
    {
        var result = _ministerService.AddAndSave((Entities.Minister)model);
        var minister = _ministerService.FindById(result.Id) ?? throw new NoContentException("Minister does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new MinisterModel(minister));
    }

    /// <summary>
    /// Update minister for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MinisterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Minister" })]
    public IActionResult Update([FromBody] MinisterModel model)
    {
        var result = _ministerService.UpdateAndSave((Entities.Minister)model);
        var minister = _ministerService.FindById(result.Id) ?? throw new NoContentException("Minister does not exist");
        return new JsonResult(new MinisterModel(minister));
    }

    /// <summary>
    /// Delete minister for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MinisterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Minister" })]
    public IActionResult Delete([FromBody] MinisterModel model)
    {
        _ministerService.DeleteAndSave((Entities.Minister)model);
        return new JsonResult(model);
    }
    #endregion
}
