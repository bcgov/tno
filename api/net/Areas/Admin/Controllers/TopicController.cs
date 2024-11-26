using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Topic;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// TopicController class, provides topic endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(new[] { ClientRole.Editor, ClientRole.Administrator })]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/topics")]
[Route("api/[area]/topics")]
[Route("v{version:apiVersion}/[area]/topics")]
[Route("[area]/topics")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TopicController : ControllerBase
{
    #region Variables
    private readonly ITopicService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public TopicController(ITopicService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of topic for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().OrderByDescending((k) => k.TopicType).ThenBy((k) => k.Name).Select(ds => new TopicModel(ds)));
    }

    /// <summary>
    /// Find a page of topic for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<TopicModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new TopicFilter(query));
        var page = new Paged<TopicModel>(result.Items.Select(ds => new TopicModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find topic for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new TopicModel(result));
    }

    /// <summary>
    /// Add a topic.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult Add([FromBody] TopicModel model)
    {
        var result = _service.AddAndSave((Topic)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new TopicModel(result));
    }

    /// <summary>
    /// Update topic for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult Update([FromBody] TopicModel model)
    {
        var result = _service.UpdateAndSave((Topic)model);
        return new JsonResult(new TopicModel(result));
    }

    /// <summary>
    /// Delete topic for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Topic" })]
    public IActionResult Delete([FromBody] TopicModel model)
    {
        _service.DeleteAndSave((Topic)model);
        return new JsonResult(model);
    }
    #endregion
}
