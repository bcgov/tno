namespace TNO.API.Areas.Services.Models.Ingest;

/// <summary>
/// IngestScheduleModel class, provides a model that represents an ingest schedule (many-to-many).
/// </summary>
public class IngestScheduleModel
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int IngestId { get; set; }

    /// <summary>
    /// get/set - The foreign key to the parent schedule.
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// get/set - The schedule.
    /// </summary>
    public ScheduleModel? Schedule { get; set; }
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
    public IngestScheduleModel(Entities.IngestSchedule entity)
    {
        this.IngestId = entity.IngestId;
        this.ScheduleId = entity.ScheduleId;
        this.Schedule = entity.Schedule != null ? new ScheduleModel(entity.Schedule) : null;
    }

    /// <summary>
    /// Creates a new instance of an IngestScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="ingestId"></param>
    /// <param name="model"></param>
    public IngestScheduleModel(int ingestId, ScheduleModel model)
    {
        this.IngestId = ingestId;
        this.ScheduleId = model.Id;
        this.Schedule = model;
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
        return new Entities.IngestSchedule(model.IngestId, model.ScheduleId)
        {
            Schedule = model.Schedule != null ? (Entities.Schedule)model.Schedule : null
        };
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Schedule(IngestScheduleModel model)
    {
        if (model.Schedule == null) throw new ArgumentNullException(nameof(model));
        return (Entities.Schedule)model.Schedule;
    }
    #endregion
}
