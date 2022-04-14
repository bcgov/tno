package ca.bc.gov.tno.services.data;

import java.time.LocalDate;
import java.time.LocalTime;
import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.models.ScheduleMonths;
import ca.bc.gov.tno.services.models.ScheduleWeekDays;

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

    var runAtCal = GregorianCalendar.from(runAt);
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
  public static boolean verifySchedule(ZonedDateTime now, DataSourceConfig dataSource, ScheduleConfig schedule) {
    var cal = GregorianCalendar.from(now);

    var isEnabled = dataSource.getIsEnabled();
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
    var next = GregorianCalendar.from(dataSource.getLastRanOn());
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
        && (runOn == null || dataSource.getLastRanOn().isAfter(runOn)))
      return false;

    // No limitation imposed by runOn, so always run.
    if (runOn == null)
      return true;

    var runOnCal = GregorianCalendar.from(runOn);

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

    if (runOnWeekDays == null || runOnWeekDays.contains(ScheduleWeekDays.NA))
      return true;

    switch (dayOfWeek) {
      case (1):
        return runOnWeekDays.contains(ScheduleWeekDays.Monday);
      case (2):
        return runOnWeekDays.contains(ScheduleWeekDays.Tuesday);
      case (3):
        return runOnWeekDays.contains(ScheduleWeekDays.Wednesday);
      case (4):
        return runOnWeekDays.contains(ScheduleWeekDays.Thursday);
      case (5):
        return runOnWeekDays.contains(ScheduleWeekDays.Friday);
      case (6):
        return runOnWeekDays.contains(ScheduleWeekDays.Saturday);
      case (7):
        return runOnWeekDays.contains(ScheduleWeekDays.Sunday);
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

    if (runOnMonth == null || runOnMonth.contains(ScheduleMonths.NA))
      return true;

    switch (month) {
      case (0):
        return runOnMonth.contains(ScheduleMonths.January);
      case (1):
        return runOnMonth.contains(ScheduleMonths.February);
      case (2):
        return runOnMonth.contains(ScheduleMonths.March);
      case (3):
        return runOnMonth.contains(ScheduleMonths.April);
      case (4):
        return runOnMonth.contains(ScheduleMonths.May);
      case (5):
        return runOnMonth.contains(ScheduleMonths.June);
      case (6):
        return runOnMonth.contains(ScheduleMonths.July);
      case (7):
        return runOnMonth.contains(ScheduleMonths.August);
      case (8):
        return runOnMonth.contains(ScheduleMonths.September);
      case (9):
        return runOnMonth.contains(ScheduleMonths.October);
      case (10):
        return runOnMonth.contains(ScheduleMonths.November);
      case (11):
        return runOnMonth.contains(ScheduleMonths.December);
    }
    return false;
  }

  /**
   * Convert a LocalTime object to a unix epoch date/time in the current time
   * zone.
   *
   * @param time     The LocalTime object to convert
   * @param timeZone The timezone.
   * @return the date/time in milliseconds
   */
  public static long getMsDateTime(LocalTime time, String timeZone) {

    var dateTime = time.atDate(LocalDate.now(ZoneId.of(timeZone)));
    var instant = dateTime.atZone(ZoneId.of(timeZone)).toInstant();
    var timeInMillis = instant.toEpochMilli();

    return timeInMillis;
  }
}
