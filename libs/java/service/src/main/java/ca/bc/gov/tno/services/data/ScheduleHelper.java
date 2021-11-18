package ca.bc.gov.tno.services.data;

import java.util.Calendar;
import java.util.Date;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * ScheduleHelper static class, provides helper methods for the scheduler.
 */
public final class ScheduleHelper {

  /**
   * Determine if the object is a BaseScheduleService.
   * 
   * @param object The object to check.
   * @return True if the object is a BaseScheduleService.
   */
  public static boolean isSchedule(Object object) {
    return object instanceof BaseScheduleService<?, ?>;
  }

  /**
   * Calculate the time to wait until the next day's runAt value.
   * 
   * @param config Configuration settings.
   * @return Number of milliseconds to wait before running again.
   */
  public static long calcWait(DataSourceConfig config) {
    var runAt = config.getRunAt();

    if (runAt == null)
      return 0;

    var runAtCal = Calendar.getInstance();
    runAtCal.setTime(runAt);
    var runAtHour = runAtCal.get(Calendar.HOUR_OF_DAY);
    var runAtMinute = runAtCal.get(Calendar.MINUTE);
    runAtCal.add(Calendar.DATE, 1);
    runAtCal.set(Calendar.HOUR, runAtHour);
    runAtCal.set(Calendar.MINUTE, runAtMinute);
    runAtCal.set(Calendar.SECOND, 0);

    var nowCal = Calendar.getInstance();
    var now = new Date(System.currentTimeMillis());
    nowCal.setTime(now);

    return runAtCal.getTimeInMillis() - nowCal.getTimeInMillis();
  }

  /**
   * Determine if the schedule allows for the process to run at this point in
   * time.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifySchedule(Date now, DataSourceConfig config) {
    var cal = Calendar.getInstance();
    cal.setTime(now);

    var isEnabled = config.isEnabled();
    var isRun = verifyDelay(cal, config);
    var isRunAt = verifyRunAt(cal, config);
    var isDayOfMonth = verifyDayOfMonth(cal, config);
    var isWeekDay = verifyWeekDay(cal, config);
    var isMonth = verifyMonth(cal, config);
    return isEnabled && isRun && isRunAt && isDayOfMonth && isWeekDay && isMonth;
  }

  /**
   * Determine if the scheduled delay has been exceeded.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyDelay(Calendar now, DataSourceConfig config) {
    var delay = config.getDelay();
    var lastRanOn = config.getLastRanOn();

    if (delay == 0 || lastRanOn == null)
      return true;

    // Add the delay to the last ran on to determine if it should run again.
    var next = Calendar.getInstance();
    next.setTime(config.getLastRanOn());
    next.add(Calendar.MILLISECOND, delay);

    return next.before(now);
  }

  /**
   * Verify that the process can run at this time in the day.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyRunAt(Calendar now, DataSourceConfig config) {
    var runAt = config.getRunAt();

    // TODO: Make this work across multiple instances.
    // If the data source has been run its max repeat limit, don't run again.
    if (config.getRepeat() > 0 && config.getRanCounter() >= config.getRepeat()
        && (runAt == null || config.getLastRanOn().after(runAt)))
      return false;

    // No limitation imposed by runAt, so always run.
    if (runAt == null)
      return true;

    var runAtCal = Calendar.getInstance();
    runAtCal.setTime(runAt);

    // If runAt is in the future don't run.
    if (runAtCal.after(now))
      return false;

    // If runAt is in the past we are only interested in the time.
    var runAtHour = runAtCal.get(Calendar.HOUR_OF_DAY);
    var runAtMinute = runAtCal.get(Calendar.MINUTE);
    var hour = now.get(Calendar.HOUR_OF_DAY);
    var minute = now.get(Calendar.MINUTE);

    return (runAtHour < hour) || (runAtHour == hour && runAtMinute <= minute);
  }

  /**
   * Verify that the process can run on this day of the month.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyDayOfMonth(Calendar now, DataSourceConfig config) {
    var dayOfMonth = now.get(Calendar.DAY_OF_MONTH);
    var runOnDayOfMonth = config.getDayOfMonth();

    return runOnDayOfMonth == 0 || dayOfMonth == runOnDayOfMonth;
  }

  /**
   * Verify that the process can run on this day of the week.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyWeekDay(Calendar now, DataSourceConfig config) {
    var dayOfWeek = now.get(Calendar.DAY_OF_WEEK);
    var runOnWeekDays = config.getRunOnWeekDays();

    if (runOnWeekDays == null || runOnWeekDays.contains(WeekDays.NA))
      return true;

    switch (dayOfWeek) {
    case (1):
      return runOnWeekDays.contains(WeekDays.Monday);
    case (2):
      return runOnWeekDays.contains(WeekDays.Tuesday);
    case (3):
      return runOnWeekDays.contains(WeekDays.Wednesday);
    case (4):
      return runOnWeekDays.contains(WeekDays.Thursday);
    case (5):
      return runOnWeekDays.contains(WeekDays.Friday);
    case (6):
      return runOnWeekDays.contains(WeekDays.Saturday);
    case (7):
      return runOnWeekDays.contains(WeekDays.Sunday);
    }
    return false;
  }

  /**
   * Verify that the process can run on this month.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyMonth(Calendar now, DataSourceConfig config) {
    var month = now.get(Calendar.MONTH);
    var runOnMonth = config.getRunOnMonths();

    if (runOnMonth == null || runOnMonth.contains(Months.NA))
      return true;

    switch (month) {
    case (0):
      return runOnMonth.contains(Months.January);
    case (1):
      return runOnMonth.contains(Months.February);
    case (2):
      return runOnMonth.contains(Months.March);
    case (3):
      return runOnMonth.contains(Months.April);
    case (4):
      return runOnMonth.contains(Months.May);
    case (5):
      return runOnMonth.contains(Months.June);
    case (6):
      return runOnMonth.contains(Months.July);
    case (7):
      return runOnMonth.contains(Months.August);
    case (8):
      return runOnMonth.contains(Months.September);
    case (9):
      return runOnMonth.contains(Months.October);
    case (10):
      return runOnMonth.contains(Months.November);
    case (11):
      return runOnMonth.contains(Months.December);
    }
    return false;
  }
}
