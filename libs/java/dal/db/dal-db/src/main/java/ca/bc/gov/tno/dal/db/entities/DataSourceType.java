package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * DataSourceType class, provides a way to identify the different data source
 * types.
 */
@Entity
@Table(name = "\"DataSourceType\"")
public class DataSourceType extends AuditColumns {
  /**
   * Primary key to identify the data source type.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_DataSourceType")
  @SequenceGenerator(name = "seq_DataSourceType", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the data source type.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the data source type.
   */
  @Column(name = "\"description\"")
  private String description;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled;

  /**
   * A collection of data sources of this type.
   */
  @JsonBackReference
  @OneToMany(mappedBy = "type", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * Creates a new instance of a DataSourceType object.
   */
  public DataSourceType() {

  }

  /**
   * Creates a new instance of a DataSourceType object, initializes with specified
   * parameters.
   * 
   * @param id   Primary key
   * @param name Unique name
   */
  public DataSourceType(int id, String name) {
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
   * @return List{DataSource} return the dataSources
   */
  public List<DataSource> getDataSources() {
    return dataSources;
  }

}
