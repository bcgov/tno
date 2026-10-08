using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.TopicScore;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// TopicScoreController class, the Event Handler's API for bulk rescore work orders.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/topic-scores")]
[Route("api/[area]/topic-scores")]
[Route("v{version:apiVersion}/[area]/topic-scores")]
[Route("[area]/topic-scores")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TopicScoreController : ControllerBase
{
    #region Variables
    private readonly ITopicScoreService _scoreService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreController object, initializes with specified parameters.
    /// </summary>
    /// <param name="scoreService"></param>
    public TopicScoreController(ITopicScoreService scoreService)
    {
        _scoreService = scoreService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Rescore the next page of a bulk rescore after the specified content ID. Content whose score
    /// changed is re-indexed before the API responds.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="after"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    [HttpPost("rescores/{workOrderId:long}/pages")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicRescorePageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult RescorePage(long workOrderId, long after = 0, int quantity = 200)
    {
        var workOrder = _scoreService.FindRescore(workOrderId);
        if (workOrder == null) return NoContent();
        return new JsonResult(_scoreService.RescorePage(TopicScoreService.ReadRescoreConfiguration(workOrder), after, quantity));
    }
    #endregion
}
