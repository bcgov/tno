using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.API.Models;
using TNO.Core.Extensions;
using TNO.DAL.Services;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// DataSourceController class, provides DataSource endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/data/sources")]
[Route("api/[area]/data/sources")]
[Route("v{version:apiVersion}/[area]/data/sources")]
[Route("[area]/data/sources")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class DataSourceController : ControllerBase
{
    #region Variables
    private readonly IDataSourceService _serviceDataSource;
    private readonly IScheduleService _serviceSchedule;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceDataSource"></param>
    /// <param name="serviceSchedule"></param>
    /// <param name="serializerOptions"></param>
    public DataSourceController(IDataSourceService serviceDataSource, IScheduleService serviceSchedule, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceDataSource = serviceDataSource;
        _serviceSchedule = serviceSchedule;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{code}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindByCode(string code)
    {
        var result = _serviceDataSource.FindByCode(code);
        if (result == null) return new NoContentResult();
        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("for/media/type/{mediaTypeName}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DataSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindByMediaType(string mediaTypeName)
    {
        var result = _serviceDataSource.FindByMediaType(mediaTypeName);
        return new JsonResult(result.Select(ds => new DataSourceModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Update data-source in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Update(DataSourceModel model)
    {
        var result = _serviceDataSource.Update(model.ToEntity(_serializerOptions));

        // Update any schedules that have changed.
        // TODO: This is a bad design, services should not be changing the schedule.
        model.DataSourceSchedules.ForEach(dss =>
        {
            if (dss.Schedule != null)
            {
                var schedule = result.SchedulesManyToMany.FirstOrDefault(s => s.ScheduleId == dss.ScheduleId)?.Schedule;
                if (schedule != null && schedule.RunOn != dss.Schedule.RunOn)
                {
                    schedule.RunOn = dss.Schedule.RunOn;
                    _serviceSchedule.Update(schedule);
                }
            }
        });

        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }
    #endregion
}
