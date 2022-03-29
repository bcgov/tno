package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * DataSourceAction class, provides a way to manage dataSource actions.
 */
@Entity
@IdClass(DataSourceActionPK.class)
@Table(name = "data_source_action", schema = "public")
public class DataSourceAction extends AuditColumns {
  /**
   * Primary key to identify the dataSource action.
   * Foreign key to dataSource.
   */
  @Id
  @Column(name = "data_source_id", nullable = false)
  private int dataSourceId;

  /**
   * The dataSource reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "data_source_id", insertable = false, updatable = false)
  private DataSource dataSource;

  /**
   * Primary key to identify the dataSource action.
   * Foreign key to action .
   */
  @Id
  @Column(name = "source_action_id", nullable = false)
  private int sourceActionId;

  /**
   * The action reference.
   */
  @JsonBackReference("sourceAction")
  @ManyToOne(fetch = FetchType.EAGER)
  @JoinColumn(name = "source_action_id", insertable = false, updatable = false)
  private SourceAction sourceAction;

  /**
   * Value of action.
   */
  @Column(name = "value", nullable = false)
  private boolean value;

  /**
   * Creates a new instance of a DataSourceAction object.
   */
  public DataSourceAction() {

  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource DataSource object
   * @param action     SourceAction object
   * @param value      Action value
   */
  public DataSourceAction(DataSource dataSource, SourceAction action, boolean value) {
    if (dataSource == null)
      throw new NullPointerException("Parameter 'dataSource' cannot be null.");
    if (action == null)
      throw new NullPointerException("Parameter 'action' cannot be null.");

    this.dataSource = dataSource;
    this.dataSourceId = dataSource.getId();
    this.sourceAction = action;
    this.sourceActionId = action.getId();
    this.value = value;
  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource DataSource object
   * @param action     SourceAction object
   * @param value      Action value
   * @param version    Row version value
   */
  public DataSourceAction(DataSource dataSource, SourceAction action, boolean value, long version) {
    this(dataSource, action, value);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource     DataSource object
   * @param sourceActionId Foreign key to Action object
   * @param value          Action value
   */
  public DataSourceAction(DataSource dataSource, int sourceActionId, boolean value) {
    if (dataSource == null)
      throw new NullPointerException("Parameter 'dataSource' cannot be null.");

    this.dataSource = dataSource;
    this.dataSourceId = dataSource.getId();
    this.sourceActionId = sourceActionId;
    this.value = value;
  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource     DataSource object
   * @param sourceActionId Foreign key to Action object
   * @param value          Action value
   * @param version        Row version value
   */
  public DataSourceAction(DataSource dataSource, int sourceActionId, boolean value, long version) {
    this(dataSource, sourceActionId, value);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId   Foreign key to DataSource object
   * @param sourceActionId Foreign key to Action object
   * @param value          Action value
   */
  public DataSourceAction(int dataSourceId, int sourceActionId, boolean value) {
    this.dataSourceId = dataSourceId;
    this.sourceActionId = sourceActionId;
    this.value = value;
  }

  /**
   * Creates a new instance of a DataSourceAction object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId   Foreign key to DataSource object
   * @param sourceActionId Foreign key to Action object
   * @param value          Action value
   * @param version        Row version value
   */
  public DataSourceAction(int dataSourceId, int sourceActionId, boolean value, long version) {
    this(dataSourceId, sourceActionId, value);
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

  /**
   * @return Action return the sourceAction
   */
  public SourceAction getSourceAction() {
    return sourceAction;
  }

  /**
   * @param sourceAction the sourceAction to set
   */
  public void setSourceAction(SourceAction sourceAction) {
    this.sourceAction = sourceAction;
  }

  /**
   * @return boolean return the value
   */
  public boolean getValue() {
    return value;
  }

  /**
   * @param value the value to set
   */
  public void setValue(boolean value) {
    this.value = value;
  }

}
