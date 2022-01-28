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
 * License class, provides a way to manage license information for data sources.
 */
@Entity
@Table(name = "\"License\"")
public class License extends AuditColumns {
  /**
   * Primary key to identify the license.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_License")
  @SequenceGenerator(name = "seq_License", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the license.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the license.
   */
  @Column(name = "\"description\"")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled = true;

  /**
   * The number of days content is allowed to be kept before it must be purged (0
   * = forever).
   */
  @Column(name = "\"ttl\"", nullable = false)
  private int ttl;

  /**
   * A collection of data sources that belong to this license.
   */
  @JsonBackReference("dataSources")
  @OneToMany(mappedBy = "license", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * A collection of content that belong to this license.
   */
  @JsonBackReference("contents")
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
   * @param id   Primary key
   * @param name Unique name
   * @param ttl  Time to live in days
   */
  public License(int id, String name, int ttl) {
    this.id = id;
    this.name = name;
    this.ttl = ttl;
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
   * @return List{DataSource} return the dataSources
   */
  public List<DataSource> getDataSources() {
    return dataSources;
  }

}
