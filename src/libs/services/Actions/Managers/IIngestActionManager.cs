using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Services;

/// <summary>
/// IIngestActionManager interface, provides a way to manage a single ingestion service.
/// </summary>
public interface IIngestActionManager : IServiceActionManager
{
    #region Properties
    /// <summary>
    /// get - The ingest managed by this object.
    /// </summary>
    IngestModel Ingest { get; set; }

    /// <summary>
    /// get - A dictionary of values that can be stored with this manager.
    /// </summary>
    Dictionary<string, object> Values { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Make AJAX request and update the ingest state - Failed Attempts.
    /// </summary>
    /// <param name="failedAttempts"></param>
    /// <param name="lastItemUpdatedDate"></param>
    /// <returns></returns>
    Task<IngestModel> UpdateIngestStateFailedAttemptsAsync(int failedAttempts = 0, DateTime? lastItemUpdatedDate = null);

    /// <summary>
    /// Verify that the specified ingest ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    bool VerifyIngest();

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifySchedule(ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifySchedule(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// If the schedule type is 'Daily' or 'Advanced' the DelayMS is used to slow down the process rather than to determine if it should run.
    /// * This will always return true for those schedule types.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyDelay(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyRunOn(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date' and time.
    /// Compares the schedule 'StartAt' and 'StopAt' values with the specified 'date' and time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyStartAt(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyDayOfMonth(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyWeekDay(DateTime date, ScheduleModel schedule);

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    bool VerifyMonth(DateTime date, ScheduleModel schedule);
    #endregion
}
