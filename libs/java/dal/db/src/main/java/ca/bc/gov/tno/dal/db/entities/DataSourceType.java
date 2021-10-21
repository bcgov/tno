package ca.bc.gov.tno.dal.db.entities;

import java.util.Set;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.Table;

/**
 * DataSourceType class, provides a way to identify the different data source
 * types.
 */
@Entity
@Table(name = "\"DataSourceType\"")
public class DataSourceType extends Audit {
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

  @OneToMany(mappedBy = "type", fetch = FetchType.LAZY)
  private Set<DataSource> dataSources;

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
