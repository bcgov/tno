using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Action;
using TNO.API.Models;
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
    [Produces("application/json")]
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Add(ActionModel model)
    {
        var result = _service.Add(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Update(ActionModel model)
    {
        var result = _service.Update(model.ToEntity());
        return new JsonResult(new ActionModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Action" })]
    public IActionResult Delete(ActionModel model)
    {
        _service.Delete(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
