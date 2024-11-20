using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Action;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ActionController class, provides Action endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/actions")]
[Route("api/[area]/actions")]
[Route("v{version:apiVersion}/[area]/actions")]
[Route("[area]/actions")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ActionController : ControllerBase
{
    #region Variables
    private readonly IActionService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ActionController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ActionController(IActionService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<ActionModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult FindAll()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        return new JsonResult(_service.FindAll().Select(ds => new ActionModel(ds)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Add([FromBody] ActionModel model)
    {
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Update([FromBody] ActionModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity());
        return new JsonResult(new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Delete([FromBody] ActionModel model)
    {
        _service.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
