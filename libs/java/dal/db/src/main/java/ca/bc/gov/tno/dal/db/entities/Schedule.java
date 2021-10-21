package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;
import java.util.EnumSet;
import java.util.Set;

import javax.persistence.Column;
import javax.persistence.Convert;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.converters.WeekDaysAttributeConverter;
import ca.bc.gov.tno.dal.db.converters.MonthsAttributeConverter;

/**
 * Schedule class, provides a way to manage scheduling details for data sources.
 */
@Entity
@Table(name = "\"Schedule\"")
public class Schedule extends Audit {
  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  @Column(name = "\"name\"", nullable = false)
  private String name;

  @Column(name = "\"description\"")
  private String description;

  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean isEnabled;

  @Column(name = "\"delayMS\"", nullable = false)
  private int delayMS;

  @Column(name = "\"runAt\"")
  private Date runAt;

  @Column(name = "\"runOnWeekDays\"", nullable = false)
  @Convert(converter = WeekDaysAttributeConverter.class)
  private EnumSet<WeekDays> runOnWeekDays;

  @Column(name = "\"runOnMonths\"", nullable = false)
  @Convert(converter = MonthsAttributeConverter.class)
  private EnumSet<Months> runOnMonths;

  @Column(name = "\"dayOfMonth\"", nullable = false)
  private int dayOfMonth;

  @OneToMany(mappedBy = "schedule", fetch = FetchType.LAZY)
  private Set<DataSource> dataSources;

  /**
   * Creates a new instance of a Schedule object.
   */
  public Schedule() {

  }

  /**
   * Creates a new instance of a Schedule object, initializes with specified
   * parameters.
   * 
   * @param id
   * @param name
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
   * @return boolean return the isEnabled
   */
  public boolean isIsEnabled() {
    return isEnabled;
  }

  /**
   * @param isEnabled the isEnabled to set
   */
  public void setIsEnabled(boolean isEnabled) {
    this.isEnabled = isEnabled;
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
   * @return EnumSet<WeekDays> return the runOnWeekDays
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
   * @return EnumSet<Months> return the runOnMonths
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
   * @return Set<DataSource> return the dataSources
   */
  public Set<DataSource> getDataSources() {
    return dataSources;
  }

  /**
   * @param dataSources the dataSources to set
   */
  public void setDataSources(Set<DataSource> dataSources) {
    this.dataSources = dataSources;
  }

}
