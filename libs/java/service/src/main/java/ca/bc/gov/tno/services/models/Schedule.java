package ca.bc.gov.tno.services.models;

import java.time.LocalTime;
import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;

import org.springframework.format.annotation.DateTimeFormat;

import ca.bc.gov.tno.services.converters.Settings;
import ca.bc.gov.tno.services.converters.ZonedDateTimeDeserializer;

/**
 * Schedule class, provides a way to manage scheduling details for data sources.
 */
public class Schedule extends AuditColumns {
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
  private boolean isEnabled = true;

  /**
   * The type of schedule.
   */
  private ScheduleType scheduleType = ScheduleType.Repeating;

  /**
   * The number of milliseconds the service should rest before running again.
   */
  private int delayMS;

  /**
   * The date and time the service should begin running on. This is useful if a
   * service should be delayed from running for a period of time.
   * null = Run immediately.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = Settings.dateTimeFormat)
  @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private ZonedDateTime runOn;

  /**
   * At what time the schedule should start.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = "HH:mm:ss")
  @DateTimeFormat(pattern = "HH:mm:ss")
  private LocalTime startAt;

  /**
   * At what time the schedule should stop.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = "HH:mm:ss")
  @DateTimeFormat(pattern = "HH:mm:ss")
  private LocalTime stopAt;

  /**
   * Number of times to run before waiting for next RunAt.
   * 0 = is used for continuous running.
   */
  private int repeat;

  /**
   * Identify which week days the service should run.
   * NA = Do not use runOnWeekDays.
   */
  private EnumSet<ScheduleWeekDays> runOnWeekDays = EnumSet.of(ScheduleWeekDays.NA);

  /**
   * Identify which months the service should run.
   * NA = Do not use runOnMonths.
   */
  private EnumSet<ScheduleMonths> runOnMonths = EnumSet.of(ScheduleMonths.NA);

  /**
   * Identify the day of the month the service should run.
   * 0 = Do not use dayOfMonth.
   */
  private int dayOfMonth;

  /**
   * A collection of data sources that belong to this schedule.
   */
  private List<DataSourceSchedule> dataSourceSchedules = new ArrayList<>();

  /**
   * Creates a new instance of a Schedule object.
   */
  public Schedule() {

  }

  /**
   * Creates a new instance of a Schedule object, initializes with specified
   * parameters.
   *
   * @param name Unique name
   */
  public Schedule(String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name;
  }

  /**
   * Creates a new instance of a Schedule object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public Schedule(int id, String name) {
    this(name);
    this.id = id;
  }

  /**
   * Creates a new instance of a Schedule object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public Schedule(int id, String name, long version) {
    this(id, name);
    this.setVersion(version);
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
  public boolean getIsEnabled() {
    return isEnabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.isEnabled = enabled;
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
   * @return Date return the repeat
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

  /**
   * @return List{DataSourceSchedule} return the dataSourceSchedules
   */
  public List<DataSourceSchedule> getDataSourceSchedules() {
    return dataSourceSchedules;
  }

  /**
   * @param dataSourceSchedules the dataSourceSchedules to set
   */
  public void setDataSourceSchedules(List<DataSourceSchedule> dataSourceSchedules) {
    this.dataSourceSchedules = dataSourceSchedules;
  }

}
