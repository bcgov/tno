namespace TNO.API.Areas.Admin.Models.DataSource;

/// <summary>
/// SourceScheduleModel class, provides a model that represents an source metric.
/// </summary>
public class SourceScheduleModel : ScheduleModel
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int DataSourceId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceScheduleModel.
    /// </summary>
    public SourceScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an SourceScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceScheduleModel(Entities.DataSourceSchedule entity) : base(entity.Schedule)
    {
        this.Id = entity.ScheduleId;
        this.DataSourceId = entity.DataSourceId;
        this.CreatedBy = entity.CreatedBy;
        this.CreatedById = entity.CreatedById;
        this.CreatedOn = entity.CreatedOn;
        this.UpdatedBy = entity.UpdatedBy;
        this.UpdatedById = entity.UpdatedById;
        this.UpdatedOn = entity.UpdatedOn;
        this.Version = entity.Version;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a DataSourceSchedule object.
    /// </summary>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public Entities.DataSourceSchedule ToEntity(int dataSourceId)
    {
        var entity = (Entities.DataSourceSchedule)this;
        entity.DataSourceId = dataSourceId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.DataSourceSchedule(SourceScheduleModel model)
    {
        return new Entities.DataSourceSchedule(model.DataSourceId, model.Id)
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
