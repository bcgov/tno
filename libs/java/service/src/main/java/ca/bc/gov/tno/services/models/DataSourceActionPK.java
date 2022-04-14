package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * DataSourceActionPK class, provides primary key for DataSourceAction.
 */
public class DataSourceActionPK implements Serializable {
  /**
   * The data source action abbreviation.
   * Foreign key to data source.
   */
  private int dataSourceId;

  /**
   * The dataSource action unique key.
   * Foreign key to action.
   */
  private int sourceActionId;

  /**
   * Creates a new instance of a DataSourceActionPK object.
   */
  public DataSourceActionPK() {

  }

  /**
   * Creates a new instance of a DataSourceActionPK object, initializes with
   * specified parameters.
   * 
   * @param dataSourceId   Foreign key to data source.
   * @param sourceActionId Foreign key to source action.
   */
  public DataSourceActionPK(int dataSourceId, int sourceActionId) {
    this.dataSourceId = dataSourceId;
    this.sourceActionId = sourceActionId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.dataSourceId);
    hash = 79 & hash + Objects.hashCode(this.sourceActionId);
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
    final DataSourceActionPK pk = (DataSourceActionPK) obj;
    if (!Objects.equals(this.dataSourceId, pk.dataSourceId)
        || !Objects.equals(this.sourceActionId, pk.sourceActionId)) {
      return false;
    }
    return Objects.equals(this.dataSourceId, pk.dataSourceId) && Objects.equals(this.sourceActionId, pk.sourceActionId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("dataSourceId=").append(dataSourceId);
    sb.append(", sourceActionId=").append(sourceActionId);
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
   * @return int return the sourceActionId
   */
  public int getSourceActionId() {
    return sourceActionId;
  }

  /**
   * @param sourceActionId the sourceActionId to set
   */
  public void setSourceActionId(int sourceActionId) {
    this.sourceActionId = sourceActionId;
  }

}
