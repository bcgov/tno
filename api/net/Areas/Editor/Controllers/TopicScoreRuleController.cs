using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.TopicScoreRule;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// TopicScoreRuleController class, provides TopicScoreRule endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
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
    /// Return an array of TopicScoreRule.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicScoreRuleModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "TopicScoreRule" })]
    [ETagCacheTableFilter("topic_score_rules")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new TopicScoreRuleModel(c)));
    }
    #endregion
}
