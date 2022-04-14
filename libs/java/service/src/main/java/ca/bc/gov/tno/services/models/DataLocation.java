package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * DataLocation class, provides a way to identify the different data locations.
 */
public class DataLocation extends AuditColumns {
  /**
   * Primary key to identify the data location.
   */
  private int id;

  /**
   * A unique name to identify the data location.
   */
  private String name;

  /**
   * A description of the data location.
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
   * A collection of data sources of this type.
   */
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
   * @return boolean return the isEnabled
   */
  public boolean getIsEnabled() {
    return isEnabled;
  }

  /**
   * @param isEnabled the isEnabled to set
   */
  public void setIsEnabled(boolean isEnabled) {
    this.isEnabled = isEnabled;
  }

}
