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
 * License class, provides a way to manage license information for data sources.
 */
@Entity
@Table(name = "license", schema = "public")
public class License extends AuditColumns {
  /**
   * Primary key to identify the license.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_license")
  @SequenceGenerator(name = "seq_license", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the license.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A description of the license.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean isEnabled = true;

  /**
   * The number of days content is allowed to be kept before it must be purged (0
   * = forever).
   */
  @Column(name = "ttl", nullable = false)
  private int ttl;

  /**
   * A collection of data sources that belong to this license.
   */
  @JsonIgnore
  @OneToMany(mappedBy = "license", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * A collection of content that belong to this license.
   */
  @JsonIgnore
  @OneToMany(mappedBy = "license", fetch = FetchType.LAZY)
  private List<Content> contents = new ArrayList<>();

  /**
   * Creates a new instance of a License object.
   */
  public License() {

  }

  /**
   * Creates a new instance of a License object, initializes with specified
   * parameters.
   *
   * @param name Unique name
   * @param ttl  Time to live in days
   */
  public License(String name, int ttl) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name;
    this.ttl = ttl;
  }

  /**
   * Creates a new instance of a License object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   * @param ttl  Time to live in days
   */
  public License(int id, String name, int ttl) {
    this(name, ttl);
    this.id = id;
  }

  /**
   * Creates a new instance of a License object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param ttl     Time to live in days
   * @param version Row version value
   */
  public License(int id, String name, int ttl, long version) {
    this(id, name, ttl);
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
    return isEnabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.isEnabled = enabled;
  }

  /**
   * @return int return the ttl
   */
  public int getTtl() {
    return ttl;
  }

  /**
   * @param ttl the ttl to set
   */
  public void setTtl(int ttl) {
    this.ttl = ttl;
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
