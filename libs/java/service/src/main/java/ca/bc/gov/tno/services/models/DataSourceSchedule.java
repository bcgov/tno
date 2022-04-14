package ca.bc.gov.tno.services.models;

/**
 * DataSourceSchedule class, provides a way to manage the data source schedules.
 */
public class DataSourceSchedule extends AuditColumns {
  /**
   * Primary key to identify the data source.
   * Foreign key to data source.
   */
  private int dataSourceId;

  /**
   * The data source reference.
   */
  private DataSource dataSource;

  /**
   * Primary key to identify the schedule.
   * Foreign key to schedule .
   */
  private int scheduleId;

  /**
   * The schedule reference.
   */
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
   * @param dataSource DataSource object
   * @param schedule   Schedule object
   * @param version    Row version value
   */
  public DataSourceSchedule(DataSource dataSource, Schedule schedule, long version) {
    this(dataSource, schedule);
    this.setVersion(version);
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
   * Creates a new instance of a DataSourceSchedule object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId Foreign key to data source.
   * @param scheduleId   Foreign key to Schedule.
   * @param version      Row version value
   */
  public DataSourceSchedule(int dataSourceId, int scheduleId, long version) {
    this(dataSourceId, scheduleId);
    this.setVersion(version);
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
