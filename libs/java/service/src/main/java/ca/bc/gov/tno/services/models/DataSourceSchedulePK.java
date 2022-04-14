package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * DataSourceSchedulePK class, provides primary key for DataSourceSchedule.
 */
public class DataSourceSchedulePK implements Serializable {
  /**
   * The content tag abbreviation.
   * Foreign key to data source.
   */
  private int dataSourceId;

  /**
   * The content tag unique key.
   * Foreign key to schedule.
   */
  private int scheduleId;

  /**
   * Creates a new instance of a DataSourceSchedulePK object.
   */
  public DataSourceSchedulePK() {

  }

  /**
   * Creates a new instance of a DataSourceSchedulePK object, initializes with
   * specified parameters.
   * 
   * @param dataSourceId Foreign key to data source.
   * @param scheduleId   Foreign key to schedule.
   */
  public DataSourceSchedulePK(int dataSourceId, int scheduleId) {
    this.dataSourceId = dataSourceId;
    this.scheduleId = scheduleId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.dataSourceId);
    hash = 79 & hash + Objects.hashCode(this.scheduleId);
    return hash;
  }

  @Override
  public boolean equals(Object obj) {
    if (this == obj) {
      return true;
    }
    if (obj == null) {
      return false;
    }
    if (getClass() != obj.getClass()) {
      return false;
    }
    final DataSourceSchedulePK pk = (DataSourceSchedulePK) obj;
    if (!Objects.equals(this.dataSourceId, pk.dataSourceId) || !Objects.equals(this.scheduleId, pk.scheduleId)) {
      return false;
    }
    return Objects.equals(this.dataSourceId, pk.dataSourceId) && Objects.equals(this.scheduleId, pk.scheduleId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("dataSourceId=").append(dataSourceId);
    sb.append(", scheduleId=").append(scheduleId);
    sb.append("}");
    return sb.toString();
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
   * @return int return the scheduleId
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

}
