using TNO.Core.Extensions;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// SourceScheduleModel class, provides a model that represents an source metric.
/// </summary>
public class SourceScheduleModel
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int DataSourceId { get; set; }

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
    /// Creates a new instance of an SourceScheduleModel.
    /// </summary>
    public SourceScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an SourceScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceScheduleModel(Entities.DataSourceSchedule entity)
    {
        this.DataSourceId = entity.DataSourceId;
        this.ScheduleId = entity.ScheduleId;
        this.Schedule = entity.Schedule != null ? new ScheduleModel(entity.Schedule) : null;
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
        return new Entities.DataSourceSchedule(model.DataSourceId, model.ScheduleId)
        {
            Schedule = model.Schedule != null ? (Entities.Schedule)model.Schedule : null
        };
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Schedule(SourceScheduleModel model)
    {
        if (model.Schedule == null) throw new ArgumentNullException(nameof(model));
        return (Entities.Schedule)model.Schedule;
    }
    #endregion
}
