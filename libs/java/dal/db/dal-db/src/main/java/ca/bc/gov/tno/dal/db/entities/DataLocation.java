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

import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * DataLocation class, provides a way to identify the different data locations.
 */
@Entity
@Table(name = "data_location", schema = "public")
public class DataLocation extends AuditColumns {
  /**
   * Primary key to identify the data location.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_data_location")
  @SequenceGenerator(name = "seq_data_location", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the data location.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A description of the data location.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of data sources of this type.
   */
  @JsonIgnore
  @OneToMany(mappedBy = "dataLocation", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * Creates a new instance of a DataLocation object.
   */
  public DataLocation() {

  }

  /**
   * Creates a new instance of a DataLocation object, initializes with specified
   * parameters.
   *
   * @param name Unique name
   */
  public DataLocation(String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name;
  }

  /**
   * Creates a new instance of a DataLocation object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public DataLocation(int id, String name) {
    this(name);
    this.id = id;
  }

  /**
   * Creates a new instance of a DataLocation object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public DataLocation(int id, String name, long version) {
    this(id, name);
    this.setVersion(version);
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
  public boolean getIsEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return List{DataSource} return the data_sources
   */
  public List<DataSource> getDataSources() {
    return dataSources;
  }

  /**
   * @param dataSources the dataSources to set
   */
  public void setDataSources(List<DataSource> dataSources) {
    this.dataSources = dataSources;
  }

}
