package ca.bc.gov.tno.areas.editor.models;

import java.time.LocalTime;
import java.time.ZonedDateTime;
import java.util.EnumSet;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.ScheduleType;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.entities.Schedule;
import ca.bc.gov.tno.models.AuditColumnModel;

public class ScheduleModel extends AuditColumnModel {
  /**
   * Primary key to identify the schedule.
   */
  private int id;

  /**
   * A unique name to identify the schedule.
   */
  private String name;

  /**
   * A description of the schedule.
   */
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * The schedule type.
   */
  private ScheduleType scheduleType = ScheduleType.Repeating;

  /**
   * The number of milliseconds the service should rest before running again.
   */
  private int delayMS;

  /**
   * The date and time the service should begin running on. This is useful if a
   * service should be delayed from running for a period of time.
   */
  private ZonedDateTime runOn;

  /**
   * The time to start.
   */
  private LocalTime startAt;

  /**
   * The time to stop.
   */
  private LocalTime stopAt;

  /**
   * Number of times to run before waiting for next RunAt. "0" is used for
   * continuous running.
   */
  private int repeat;

  /**
   * Identify which week days the service should run.
   */
  private EnumSet<WeekDays> runOnWeekDays;

  /**
   * Identify which months the service should run.
   */
  private EnumSet<Months> runOnMonths;

  /**
   * Identify the day of the month the service should run.
   */
  private int dayOfMonth;

  /**
   * A collection of data sources that belong to this schedule.
   */
  // private List<DataSource> data_sources = new ArrayList<>();

  public ScheduleModel() {
  }

  public ScheduleModel(Schedule entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.isEnabled();
      this.scheduleType = entity.getScheduleType();
      this.delayMS = entity.getDelayMS();
      this.runOn = entity.getRunOn();
      this.startAt = entity.getStartAt();
      this.stopAt = entity.getStopAt();
      this.repeat = entity.getRepeat();
      this.runOnWeekDays = entity.getRunOnWeekDays();
      this.runOnMonths = entity.getRunOnMonths();
      this.dayOfMonth = entity.getDayOfMonth();
    }
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
   * @return String return the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
  }

  /**
   * @return boolean return the enabled
   */
  public boolean getEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void isEnabled(boolean enabled) {
    this.enabled = enabled;
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

  /**
   * @return EnumSet{WeekDays} return the runOnWeekDays
   */
  public EnumSet<WeekDays> getRunOnWeekDays() {
    return runOnWeekDays;
  }

  /**
   * @param runOnWeekDays the runOnWeekDays to set
   */
  public void setRunOnWeekDays(EnumSet<WeekDays> runOnWeekDays) {
    this.runOnWeekDays = runOnWeekDays;
  }

  /**
   * @return EnumSet{Months} return the runOnMonths
   */
  public EnumSet<Months> getRunOnMonths() {
    return runOnMonths;
  }

  /**
   * @param runOnMonths the runOnMonths to set
   */
  public void setRunOnMonths(EnumSet<Months> runOnMonths) {
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

}
