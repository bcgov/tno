using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.AVOverview;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// AVOverviewController class, provides endpoints to manage evening overviews.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/av/overviews")]
[Route("api/[area]/av/overviews")]
[Route("v{version:apiVersion}/[area]/av/overviews")]
[Route("[area]/av/overviews")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewTemplateService _overviewTemplateService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewTemplateService"></param>
    public AVOverviewController(
        IAVOverviewTemplateService overviewTemplateService)
    {
        _overviewTemplateService = overviewTemplateService;

    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find evening overviews for the specified 'publishedOn'.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewTemplateModel[]), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindAll()
    {
        var templates = _overviewTemplateService.FindAll();
        return new JsonResult(templates.Select(t => new AVOverviewTemplateModel(t)).ToArray());
    }

    /// <summary>
    /// Find evening overview for the specified 'id'.
    /// </summary>
    /// <param name="templateType"></param>
    /// <returns></returns>
    [HttpGet("{templateType}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindById(Entities.AVOverviewTemplateType templateType)
    {
        var result = _overviewTemplateService.FindById(templateType) ?? throw new NoContentException();
        return new JsonResult(new AVOverviewTemplateModel(result));
    }

    /// <summary>
    /// Add new evening overview.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewTemplateModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Add([FromBody] AVOverviewTemplateModel model)
    {
        var result = _overviewTemplateService.AddAndSave((Entities.AVOverviewTemplate)model);
        var template = _overviewTemplateService.FindById(result.TemplateType) ?? throw new InvalidOperationException("Overview Section does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.TemplateType }, new AVOverviewTemplateModel(template));
    }

    /// <summary>
    /// Update evening overview for the specified 'templateType'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{templateType}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Update([FromBody] AVOverviewTemplateModel model)
    {
        // update the template the user is editing
        var result = _overviewTemplateService.UpdateAndSave((Entities.AVOverviewTemplate)model);

        // get a list of sibling template types we need to update
        var otherTemplateTypes = Enum.GetValues(typeof(Entities.AVOverviewTemplateType)).Cast<Entities.AVOverviewTemplateType>().ToList();
        otherTemplateTypes.Remove(model.TemplateType);

        // get the list of UserIds that have been updated
        foreach (var templateType in otherTemplateTypes)
        {
            // retrieve the sibling template
            var otherTemplate = _overviewTemplateService.FindById(templateType) ?? throw new InvalidOperationException("Overview Template does not exist");
            // update the subscription status for matching users
            foreach (var subscriber in result.SubscribersManyToMany)
            {
                var otherTemplateSubscriber = otherTemplate.SubscribersManyToMany.FirstOrDefault((s) => s.UserId == subscriber.UserId);
                if (otherTemplateSubscriber != null)
                {
                    otherTemplateSubscriber.IsSubscribed = subscriber.IsSubscribed;
                    otherTemplateSubscriber.SendTo = subscriber.SendTo;
                }
                else
                {
                    otherTemplate.SubscribersManyToMany.Add(new Entities.UserAVOverview(subscriber.UserId, subscriber.TemplateType, subscriber.IsSubscribed, subscriber.SendTo));
                }
            }

            // delete any users that no longer exist - might occur if the user was deleted completely...?
            foreach (var subscriber in otherTemplate.SubscribersManyToMany)
            {
                var deletedSubscriber = result.SubscribersManyToMany.FirstOrDefault((s) => s.UserId == subscriber.UserId);
                if (deletedSubscriber == null) otherTemplate.SubscribersManyToMany.Remove(subscriber);
            }
            _overviewTemplateService.UpdateAndSave(otherTemplate);
        }

        // retrieve the template that was initially slated for update
        result = _overviewTemplateService.FindById(model.TemplateType) ?? throw new InvalidOperationException("Overview Template does not exist");
        return new JsonResult(new AVOverviewTemplateModel(result));
    }

    /// <summary>
    /// Delete evening overview for the specified 'templateType'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{templateType}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Delete([FromBody] AVOverviewTemplateModel model)
    {
        _overviewTemplateService.DeleteAndSave((Entities.AVOverviewTemplate)model);
        return new JsonResult(model);
    }
    #endregion
}
