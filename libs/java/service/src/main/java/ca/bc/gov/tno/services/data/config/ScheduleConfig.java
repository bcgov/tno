package ca.bc.gov.tno.services.data.config;

import java.time.LocalTime;
import java.time.ZonedDateTime;
import java.util.EnumSet;

import ca.bc.gov.tno.services.models.Schedule;
import ca.bc.gov.tno.services.models.ScheduleMonths;
import ca.bc.gov.tno.services.models.ScheduleType;
import ca.bc.gov.tno.services.models.ScheduleWeekDays;

/**
 * Configuration settings for Schedule Schedule.
 */
public class ScheduleConfig {
  /**
   * Schedule key to identify what configuration to use in the database.
   */
  private int id;

  /**
   * The schedule name.
   */
  private String name;

  /**
   * The schedule type.
   */
  private ScheduleType scheduleType;

  /**
   * Millisecond delay between each request.
   */
  private int delayMS;

  /**
   * Whether the schedule is enabled.
   */
  private boolean enabled = true;

  /**
   * Date and time to run the service on.
   */
  private ZonedDateTime runOn;

  /**
   * Date and time to start the service at.
   */
  private LocalTime startAt;

  /**
   * Date and time to stop the service at.
   */
  private LocalTime stopAt;

  /**
   * Number of time to run.
   */
  private int repeat;

  /**
   * Days of week to run on.
   */
  private EnumSet<ScheduleWeekDays> runOnWeekDays;

  /**
   * Months to run on.
   */
  private EnumSet<ScheduleMonths> runOnMonths;

  /**
   * Day of month to run on.
   */
  private int dayOfMonth;

  /**
   * Creates a new instance of a ScheduleConfig object.
   */
  public ScheduleConfig() {

  }

  /**
   * Creates a new instance of a ScheduleConfig object, initializes with
   * specified parameters.
   *
   * @param schedule The schedule object.
   */
  public ScheduleConfig(Schedule schedule) {
    if (schedule == null)
      throw new IllegalArgumentException("Parameter 'schedule' is required.");

    setId(schedule.getId());
    setName(schedule.getName());
    setScheduleType(schedule.getScheduleType());
    setDelayMS(schedule.getDelayMS());
    setRunOn(schedule.getRunOn());
    setStartAt(schedule.getStartAt());
    setStopAt(schedule.getStopAt());
    setRepeat(schedule.getRepeat());
    setRunOnWeekDays(schedule.getRunOnWeekDays());
    setRunOnMonths(schedule.getRunOnMonths());
    setDayOfMonth(schedule.getDayOfMonth());
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return ScheduleType return the scheduleType
   */
  public ScheduleType getScheduleType() {
    return scheduleType;
  }

  /**
   * @param scheduleType the scheduleType to set
   */
  public void setScheduleType(ScheduleType scheduleType) {
    this.scheduleType = scheduleType;
  }

  /**
   * @return int return the delayMS
   */
  public int getDelayMS() {
    return delayMS;
  }

  /**
   * @param delayMS the delayMS to set
   */
  public void setDelayMS(int delayMS) {
    this.delayMS = delayMS;
  }

  /**
   * @return boolean return the enabled
   */
  public boolean getIsEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return ZonedDateTime return the runOn
   */
  public ZonedDateTime getRunOn() {
    return runOn;
  }

  /**
   * @param runOn the runOn to set
   */
  public void setRunOn(ZonedDateTime runOn) {
    this.runOn = runOn;
  }

  /**
   * @return LocalTime return the startAt
   */
  public LocalTime getStartAt() {
    return startAt;
  }

  /**
   * @param startAt the startAt to set
   */
  public void setStartAt(LocalTime startAt) {
    this.startAt = startAt;
  }

  /**
   * @return LocalTime return the stopAt
   */
  public LocalTime getStopAt() {
    return stopAt;
  }

  /**
   * @param stopAt the stopAt to set
   */
  public void setStopAt(LocalTime stopAt) {
    this.stopAt = stopAt;
  }

  /**
   * @return EnumSet{WeekDays} return the runOnWeekDays
   */
  public EnumSet<ScheduleWeekDays> getRunOnWeekDays() {
    return runOnWeekDays;
  }

  /**
   * @param runOnWeekDays the runOnWeekDays to set
   */
  public void setRunOnWeekDays(EnumSet<ScheduleWeekDays> runOnWeekDays) {
    this.runOnWeekDays = runOnWeekDays;
  }

  /**
   * @return EnumSet{Months} return the runOnMonths
   */
  public EnumSet<ScheduleMonths> getRunOnMonths() {
    return runOnMonths;
  }

  /**
   * @param runOnMonths the runOnMonths to set
   */
  public void setRunOnMonths(EnumSet<ScheduleMonths> runOnMonths) {
    this.runOnMonths = runOnMonths;
  }

  /**
   * @return int return the dayOfMonth
   */
  public int getDayOfMonth() {
    return dayOfMonth;
  }

  /**
   * @param dayOfMonth the dayOfMonth to set
   */
  public void setDayOfMonth(int dayOfMonth) {
    this.dayOfMonth = dayOfMonth;
  }

  /**
   * @return int return the repeat
   */
  public int getRepeat() {
    return repeat;
  }

  /**
   * @param repeat the repeat to set
   */
  public void setRepeat(int repeat) {
    this.repeat = repeat;
  }

}
