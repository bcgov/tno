package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * DataSourceMetricPK class, provides primary key for DataSourceMetric.
 */
public class DataSourceMetricPK implements Serializable {
  /**
   * The data source primary key.
   * Foreign key to data source.
   */
  private int dataSourceId;

  /**
   * The dataSource metric primary key.
   * Foreign key to metric.
   */
  private int sourceMetricId;

  /**
   * Creates a new instance of a DataSourceMetricPK object.
   */
  public DataSourceMetricPK() {

  }

  /**
   * Creates a new instance of a DataSourceMetricPK object, initializes with
   * specified parameters.
   * 
   * @param dataSourceId   Foreign key to data source.
   * @param sourceMetricId Foreign key to source metric.
   */
  public DataSourceMetricPK(int dataSourceId, int sourceMetricId) {
    this.dataSourceId = dataSourceId;
    this.sourceMetricId = sourceMetricId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.dataSourceId);
    hash = 79 & hash + Objects.hashCode(this.sourceMetricId);
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
    final DataSourceMetricPK pk = (DataSourceMetricPK) obj;
    if (!Objects.equals(this.dataSourceId, pk.dataSourceId)
        || !Objects.equals(this.sourceMetricId, pk.sourceMetricId)) {
      return false;
    }
    return Objects.equals(this.dataSourceId, pk.dataSourceId) && Objects.equals(this.sourceMetricId, pk.sourceMetricId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("dataSourceId=").append(dataSourceId);
    sb.append(", sourceMetricId=").append(sourceMetricId);
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
   * @return int return the sourceMetricId
   */
  public int getSourceMetricId() {
    return sourceMetricId;
  }

  /**
   * @param sourceMetricId the sourceMetricId to set
   */
  public void setSourceMetricId(int sourceMetricId) {
    this.sourceMetricId = sourceMetricId;
  }

}
