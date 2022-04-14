package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * SourceAction class, provides a way to identify source actions that are
 * performed on or
 * associated with a data source.
 */
public class SourceAction extends AuditColumns {
  /**
   * Primary key to identify the sourceAction.
   */
  private int id;

  /**
   * A unique name to identify the sourceAction.
   */
  private String name;

  /**
   * A description of the sourceAction.
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
   * A collection of role sourceActions that belong to this sourceAction.
   */
  private List<DataSourceAction> dataSourceActions = new ArrayList<>();

  /**
   * Creates a new instance of a SourceAction object.
   */
  public SourceAction() {

  }

  /**
   * Creates a new instance of a SourceAction object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public SourceAction(int id, String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.id = id;
    this.name = name;
  }

  /**
   * Creates a new instance of a SourceAction object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public SourceAction(int id, String name, long version) {
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
   * @return List{DataSourceAction} return the dataSourceActions
   */
  public List<DataSourceAction> getDataSourceActions() {
    return dataSourceActions;
  }

  /**
   * @param dataSourceActions the dataSourceActions to set
   */
  public void setDataSourceActions(List<DataSourceAction> dataSourceActions) {
    this.dataSourceActions = dataSourceActions;
  }

}
