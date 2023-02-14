using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.Ingest;

/// <summary>
/// IngestScheduleModel class, provides a model that represents an schedule.
/// </summary>
public class IngestScheduleModel : ScheduleModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int IngestId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestScheduleModel.
    /// </summary>
    public IngestScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an IngestScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public IngestScheduleModel(IngestSchedule entity) : base(entity.Schedule)
    {
        if (entity != null)
        {
            this.IngestId = entity.IngestId;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a IngestSchedule object.
    /// </summary>
    /// <param name="ingestId"></param>
    /// <returns></returns>
    public Entities.IngestSchedule ToEntity(int ingestId)
    {
        var entity = (Entities.IngestSchedule)this;
        entity.IngestId = ingestId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.IngestSchedule(IngestScheduleModel model)
    {
        return new Entities.IngestSchedule(model.IngestId, model.Id)
        {
            Schedule = new Entities.Schedule(model.Name, model.ScheduleType, model.DelayMS)
            {
                Id = model.Id,
                Description = model.Description,
                IsEnabled = model.IsEnabled,
                Repeat = model.Repeat,
                RunOn = model.RunOn,
                StartAt = model.StartAt,
                StopAt = model.StopAt,
                RunOnWeekDays = model.RunOnWeekDays,
                RunOnMonths = model.RunOnMonths,
                DayOfMonth = model.DayOfMonth,
                Version = model.Version ?? 0,
            }
        };
    }
    #endregion
}
