package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.ContentType;
import ca.bc.gov.tno.models.AuditColumnModel;

public class ContentTypeModel extends AuditColumnModel {
  private int id;
  private String name;
  private String description;
  private boolean enabled;
  private int sortOrder;

  public ContentTypeModel() {
  }

  public ContentTypeModel(ContentType entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.isEnabled();
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
