package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.Date;
import java.util.EnumSet;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Convert;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.ScheduleType;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.converters.WeekDaysAttributeConverter;
import ca.bc.gov.tno.dal.db.converters.MonthsAttributeConverter;

/**
 * Schedule class, provides a way to manage scheduling details for data sources.
 */
@Entity
@Table(name = "schedule", schema = "public")
public class Schedule extends AuditColumns {
  /**
   * Primary key to identify the schedule.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_schedule")
  @SequenceGenerator(name = "seq_schedule", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the schedule.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A description of the schedule.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * The type of schedule.
   */
  @Column(name = "schedule_type", nullable = false)
  private ScheduleType scheduleType = ScheduleType.Repeating;

  /**
   * The number of milliseconds the service should rest before running again.
   */
  @Column(name = "delay_ms", nullable = false)
  private int delayMS;

  /**
   * The date and time the service should begin running on. This is useful if a
   * service should be delayed from running for a period of time.
   * null = Run immediately.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "run_on")
  private Date runOn;

  /**
   * At what time the schedule should start.
   */
  @Temporal(TemporalType.TIME)
  @Column(name = "start_at")
  private Date startAt;

  /**
   * At what time the schedule should stop.
   */
  @Temporal(TemporalType.TIME)
  @Column(name = "stop_at")
  private Date stopAt;

  /**
   * Number of times to run before waiting for next RunAt.
   * 0 = is used for continuous running.
   */
  @Column(name = "repeat", nullable = false)
  private int repeat;

  /**
   * Identify which week days the service should run.
   * NA = Do not use runOnWeekDays.
   */
  @Column(name = "run_on_week_days", nullable = false)
  @Convert(converter = WeekDaysAttributeConverter.class)
  private EnumSet<WeekDays> runOnWeekDays;

  /**
   * Identify which months the service should run.
   * NA = Do not use runOnMonths.
   */
  @Column(name = "run_on_months", nullable = false)
  @Convert(converter = MonthsAttributeConverter.class)
  private EnumSet<Months> runOnMonths;

  /**
   * Identify the day of the month the service should run.
   * 0 = Do not use dayOfMonth.
   */
  @Column(name = "day_of_month", nullable = false)
  private int dayOfMonth;

  /**
   * A collection of data sources that belong to this schedule.
   */
  @JsonBackReference("data_sources")
  @OneToMany(mappedBy = "schedule", fetch = FetchType.LAZY)
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
   * @param id   Primary key
   * @param name Unique name
   */
  public Schedule(int id, String name) {
    this.id = id;
    this.name = name;
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
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
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
   * @return Date return the runOn
   */
  public Date getRunOn() {
    return runOn;
  }

  /**
   * @param runOn the runOn to set
   */
  public void setRunOn(Date runOn) {
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
   * @return Date return the startAt
   */
  public Date getStartAt() {
    return startAt;
  }

  /**
   * @param startAt the startAt to set
   */
  public void setStartAt(Date startAt) {
    this.startAt = startAt;
  }

  /**
   * @return Date return the stopAt
   */
  public Date getStopAt() {
    return stopAt;
  }

  /**
   * @param stopAt the stopAt to set
   */
  public void setStopAt(Date stopAt) {
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
