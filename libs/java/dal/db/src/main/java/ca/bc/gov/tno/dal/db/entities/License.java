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
 * License class, provides a way to manage license information for data sources.
 */
@Entity
@Table(name = "\"License\"")
public class License extends Audit {
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

  @Column(name = "\"ttl\"", nullable = false)
  private int ttl;

  @OneToMany(mappedBy = "license", fetch = FetchType.LAZY)
  private Set<DataSource> dataSources;

  /**
   * Creates a new instance of a License object.
   */
  public License() {

  }

  /**
   * Creates a new instance of a License object, initializes with specified
   * parameters.
   * 
   * @param id
   * @param name
   * @param ttl
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
