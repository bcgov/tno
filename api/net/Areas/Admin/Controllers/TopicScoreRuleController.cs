using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.TopicScoreRule;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// TopicScoreRuleController class, provides rule endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/topics/scores/rules")]
[Route("api/[area]/topics/scores/rules")]
[Route("v{version:apiVersion}/[area]/topics/scores/rules")]
[Route("[area]/topics/scores/rules")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TopicScoreRuleController : ControllerBase
{
    #region Variables
    private readonly ITopicScoreRuleService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreRuleController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public TopicScoreRuleController(ITopicScoreRuleService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of rule for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicScoreRuleModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new TopicScoreRuleModel(ds)));
    }

    /// <summary>
    /// Update all rules specified.
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    [HttpPut]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreRuleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult Update([FromBody] TopicScoreRuleModel[] models)
    {
        foreach (var rule in models)
        {
            var entity = (TopicScoreRule)rule;
            if (rule.Remove == true)
                _service.Delete(entity);
            else if (rule.Id == 0)
                _service.Add(entity);
            else
                _service.Update(entity);
        }
        _service.CommitTransaction();
        return new JsonResult(_service.FindAll().Select(ds => new TopicScoreRuleModel(ds)));
    }

    /// <summary>
    /// Find rule for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreRuleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new TopicScoreRuleModel(result));
    }

    /// <summary>
    /// Add a rule.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreRuleModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult Add([FromBody] TopicScoreRuleModel model)
    {
        var result = _service.AddAndSave((TopicScoreRule)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new TopicScoreRuleModel(result));
    }

    /// <summary>
    /// Update rule for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreRuleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult Update([FromBody] TopicScoreRuleModel model)
    {
        var result = _service.UpdateAndSave((TopicScoreRule)model);
        return new JsonResult(new TopicScoreRuleModel(result));
    }

    /// <summary>
    /// Delete rule for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreRuleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    public IActionResult Delete([FromBody] TopicScoreRuleModel model)
    {
        _service.DeleteAndSave((TopicScoreRule)model);
        return new JsonResult(model);
    }
    #endregion
}
