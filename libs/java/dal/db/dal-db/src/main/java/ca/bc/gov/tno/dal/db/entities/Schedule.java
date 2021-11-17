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
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.converters.WeekDaysAttributeConverter;
import ca.bc.gov.tno.dal.db.converters.MonthsAttributeConverter;

/**
 * Schedule class, provides a way to manage scheduling details for data sources.
 */
@Entity
@Table(name = "\"Schedule\"")
public class Schedule extends AuditColumns {
  /**
   * Primary key to identify the schedule.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_Schedule")
  @SequenceGenerator(name = "seq_Schedule", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the schedule.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the schedule.
   */
  @Column(name = "\"description\"")
  private String description;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled;

  /**
   * The number of milliseconds the service should rest before running again.
   */
  @Column(name = "\"delayMS\"", nullable = false)
  private int delayMS;

  /**
   * The date and time the service should begin running on. This is useful if a
   * service should be delayed from running for a period of time.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "\"runAt\"")
  private Date runAt;

  /**
   * Number of times to run before waiting for next RunAt. "0" is used for
   * continuous running.
   */
  @Column(name = "\"repeat\"", nullable = false)
  private int repeat;

  /**
   * Identify which week days the service should run.
   */
  @Column(name = "\"runOnWeekDays\"", nullable = false)
  @Convert(converter = WeekDaysAttributeConverter.class)
  private EnumSet<WeekDays> runOnWeekDays;

  /**
   * Identify which months the service should run.
   */
  @Column(name = "\"runOnMonths\"", nullable = false)
  @Convert(converter = MonthsAttributeConverter.class)
  private EnumSet<Months> runOnMonths;

  /**
   * Identify the day of the month the service should run.
   */
  @Column(name = "\"dayOfMonth\"", nullable = false)
  private int dayOfMonth;

  /**
   * A collection of data sources that belong to this schedule.
   */
  @JsonBackReference
  @OneToMany(mappedBy = "schedule", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

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
   * @return Date return the runAt
   */
  public Date getRunAt() {
    return runAt;
  }

  /**
   * @param runAt the runAt to set
   */
  public void setRunAt(Date runAt) {
    this.runAt = runAt;
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
   * @return List{DataSource} return the dataSources
   */
  public List<DataSource> getDataSources() {
    return dataSources;
  }

}
