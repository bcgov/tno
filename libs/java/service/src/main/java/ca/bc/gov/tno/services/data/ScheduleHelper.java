package ca.bc.gov.tno.services.data;

import java.util.Calendar;
import java.util.Date;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;

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
  public static long calcWait(ScheduleConfig config) {
    var runAt = config.getRunOn();

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
   * @param now        The date and time to verify.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @return Whether the process should be run.
   */
  public static boolean verifySchedule(Date now, DataSourceConfig dataSource, ScheduleConfig schedule) {
    var cal = Calendar.getInstance();
    cal.setTime(now);

    var isEnabled = dataSource.isEnabled();
    var isRun = verifyDelay(cal, dataSource, schedule);
    var isRunOn = verifyRunOn(cal, dataSource, schedule);
    var isDayOfMonth = verifyDayOfMonth(cal, schedule);
    var isWeekDay = verifyWeekDay(cal, schedule);
    var isMonth = verifyMonth(cal, schedule);
    return isEnabled && isRun && isRunOn && isDayOfMonth && isWeekDay && isMonth;
  }

  /**
   * Determine if the scheduled delay has been exceeded.
   * 
   * @param now        The date and time to verify.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @return Whether the process should be run.
   */
  public static boolean verifyDelay(Calendar now, DataSourceConfig dataSource, ScheduleConfig schedule) {
    var delay = schedule.getDelayMS();
    var lastRanOn = dataSource.getLastRanOn();

    if (delay == 0 || lastRanOn == null)
      return true;

    // Add the delay to the last ran on to determine if it should run again.
    var next = Calendar.getInstance();
    next.setTime(dataSource.getLastRanOn());
    next.add(Calendar.MILLISECOND, delay);

    return next.before(now);
  }

  /**
   * Verify that the process can run on this time in the day.
   * 
   * @param now        The date and time to verify.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @return Whether the process should be run.
   */
  public static boolean verifyRunOn(Calendar now, DataSourceConfig dataSource, ScheduleConfig schedule) {
    var runOn = schedule.getRunOn();

    // TODO: Make this work across multiple instances.
    // If the data source has been run its max repeat limit, don't run again.
    if (schedule.getRepeat() > 0 && dataSource.getRanCounter() >= schedule.getRepeat()
        && (runOn == null || dataSource.getLastRanOn().after(runOn)))
      return false;

    // No limitation imposed by runOn, so always run.
    if (runOn == null)
      return true;

    var runOnCal = Calendar.getInstance();
    runOnCal.setTime(runOn);

    // If runOn is in the future don't run.
    if (runOnCal.after(now))
      return false;

    // If runOn is in the past we are only interested in the time.
    var runOnHour = runOnCal.get(Calendar.HOUR_OF_DAY);
    var runOnMinute = runOnCal.get(Calendar.MINUTE);
    var hour = now.get(Calendar.HOUR_OF_DAY);
    var minute = now.get(Calendar.MINUTE);

    return (runOnHour < hour) || (runOnHour == hour && runOnMinute <= minute);
  }

  /**
   * Verify that the process can run on this day of the month.
   * 
   * @param now    The date and time to verify.
   * @param config The data source config.
   * @return Whether the process should be run.
   */
  public static boolean verifyDayOfMonth(Calendar now, ScheduleConfig config) {
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
  public static boolean verifyWeekDay(Calendar now, ScheduleConfig config) {
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
  public static boolean verifyMonth(Calendar now, ScheduleConfig config) {
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
