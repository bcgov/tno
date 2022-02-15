package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * DataSourceSchedule class, provides a way to manage the data source schedules.
 */
@Entity
@IdClass(DataSourceSchedulePK.class)
@Table(name = "data_source_schedule", schema = "public")
public class DataSourceSchedule extends AuditColumns {
  /**
   * Primary key to identify the data source.
   * Foreign key to data source.
   */
  @Id
  @Column(name = "data_source_id", nullable = false)
  private int dataSourceId;

  /**
   * The data source reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "data_source_id", insertable = false, updatable = false)
  private DataSource dataSource;

  /**
   * Primary key to identify the schedule.
   * Foreign key to schedule .
   */
  @Id
  @Column(name = "schedule_id", nullable = false)
  private int scheduleId;

  /**
   * The schedule reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "schedule_id", insertable = false, updatable = false)
  private Schedule schedule;

  /**
   * Creates a new instance of a DataSourceSchedule object.
   */
  public DataSourceSchedule() {

  }

  /**
   * Creates a new instance of a DataSourceSchedule object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource DataSource object
   * @param schedule   Schedule object
   */
  public DataSourceSchedule(DataSource dataSource, Schedule schedule) {
    if (dataSource == null)
      throw new NullPointerException("Parameter 'dataSource' cannot be null.");
    if (schedule == null)
      throw new NullPointerException("Parameter 'schedule' cannot be null.");

    this.dataSource = dataSource;
    this.dataSourceId = dataSource.getId();
    this.schedule = schedule;
    this.scheduleId = schedule.getId();
  }

  /**
   * Creates a new instance of a DataSourceSchedule object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId Foreign key to data source.
   * @param scheduleId   Foreign key to Schedule.
   */
  public DataSourceSchedule(int dataSourceId, int scheduleId) {
    this.dataSourceId = dataSourceId;
    this.scheduleId = scheduleId;
  }

  /**
   * @return int return the dataSourceId
   */
  public int getDataSourceId() {
    return dataSourceId;
  }

  /**
   * @param dataSourceId the dataSourceId to set
   */
  public void setDataSourceId(int dataSourceId) {
    this.dataSourceId = dataSourceId;
  }

  /**
   * @return DataSource return the dataSource
   */
  public DataSource getDataSource() {
    return dataSource;
  }

  /**
   * @param dataSource the dataSource to set
   */
  public void setDataSource(DataSource dataSource) {
    this.dataSource = dataSource;
  }

  /**
   * @return String return the scheduleId
   */
  public int getScheduleId() {
    return scheduleId;
  }

  /**
   * @param scheduleId the scheduleId to set
   */
  public void setScheduleId(int scheduleId) {
    this.scheduleId = scheduleId;
  }

  /**
   * @return Schedule return the schedule
   */
  public Schedule getSchedule() {
    return schedule;
  }

  /**
   * @param schedule the schedule to set
   */
  public void setSchedule(Schedule schedule) {
    this.schedule = schedule;
  }
}
