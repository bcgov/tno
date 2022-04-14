package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * License class, provides a way to manage license information for data sources.
 */
public class License extends AuditColumns {
  /**
   * Primary key to identify the license.
   */
  private int id;

  /**
   * A unique name to identify the license.
   */
  private String name;

  /**
   * A description of the license.
   */
  private String description = "";

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean isEnabled = true;

  /**
   * The number of days content is allowed to be kept before it must be purged (0
   * = forever).
   */
  private int ttl;

  /**
   * A collection of data sources that belong to this license.
   */
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * A collection of content that belong to this license.
   */
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
   * @return int return the sortOrder
   */
  public int getSortOrder() {
    return sortOrder;
  }

  /**
   * @param sortOrder the sortOrder to set
   */
  public void setSortOrder(int sortOrder) {
    this.sortOrder = sortOrder;
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

  /**
   * @return List{Content} return the contents
   */
  public List<Content> getContents() {
    return contents;
  }

  /**
   * @param contents the contents to set
   */
  public void setContents(List<Content> contents) {
    this.contents = contents;
  }

}
