using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Services;

/// <summary>
/// IIngestServiceActionManager interface, provides a way to manage a single ingestion service.
/// </summary>
public interface IIngestServiceActionManager : IServiceActionManager
{
    #region Properties
    /// <summary>
    /// get - The ingest managed by this object.
    /// </summary>
    public IngestModel Ingest { get; }

    /// <summary>
    /// get - A dictionary of values that can be stored with this manager.
    /// </summary>
    public Dictionary<string, object> Values { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Verify that the specified ingest ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public bool VerifyIngest();

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifySchedule(ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifySchedule(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// If the schedule type is 'Daily' or 'Advanced' the DelayMS is used to slow down the process rather than to determine if it should run.
    /// * This will always return true for those schedule types.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyDelay(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyRunOn(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date' and time.
    /// Compares the schedule 'StartAt' and 'StopAt' values with the specified 'date' and time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyStartAt(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyDayOfMonth(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyWeekDay(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyMonth(DateTime date, ScheduleModel schedule);
    #endregion
}
