package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.SourceAction;
import ca.bc.gov.tno.models.AuditColumnModel;

public class SourceActionModel extends AuditColumnModel {
  /**
   * Primary key to identify the action.
   */
  private int id;

  /**
   * A unique name to identify the action.
   */
  private String name;

  /**
   * A description of the action.
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
   * A collection of role actions that belong to this action.
   */
  // private List<ContentAction> contentActions = new ArrayList<>();

  public SourceActionModel() {
  }

  public SourceActionModel(SourceAction entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.getIsEnabled();
      this.sortOrder = entity.getSortOrder();
    }
  }

  /**
   * @return int the id
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
   * @return String the name
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
   * @return String the description
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
   * @return boolean the enabled
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
   * @return int the sortOrder
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

}
