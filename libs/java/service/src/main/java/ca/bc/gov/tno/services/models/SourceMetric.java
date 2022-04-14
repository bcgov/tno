package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * SourceMetric class, provides a way to identify source metrics that are
 * performed on or
 * associated with a data source.
 */
public class SourceMetric extends AuditColumns {
  /**
   * Primary key to identify the sourceMetric.
   */
  private int id;

  /**
   * A unique name to identify the sourceMetric.
   */
  private String name;

  /**
   * A description of the sourceMetric.
   */
  private String description;

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * A collection of role sourceMetrics that belong to this sourceMetric.
   */
  private List<DataSourceMetric> dataSourceMetrics = new ArrayList<>();

  /**
   * Creates a new instance of a SourceMetric object.
   */
  public SourceMetric() {

  }

  /**
   * Creates a new instance of a SourceMetric object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public SourceMetric(int id, String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.id = id;
    this.name = name;
  }

  /**
   * Creates a new instance of a SourceMetric object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public SourceMetric(int id, String name, long version) {
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
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return List{DataSourceMetric} return the dataSourceMetrics
   */
  public List<DataSourceMetric> getDataSourceMetrics() {
    return dataSourceMetrics;
  }

  /**
   * @param dataSourceMetrics the dataSourceMetrics to set
   */
  public void setDataSourceMetrics(List<DataSourceMetric> dataSourceMetrics) {
    this.dataSourceMetrics = dataSourceMetrics;
  }

}
