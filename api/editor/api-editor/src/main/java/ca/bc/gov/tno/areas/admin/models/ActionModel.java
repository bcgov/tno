package ca.bc.gov.tno.areas.admin.models;

import ca.bc.gov.tno.dal.db.ValueType;
import ca.bc.gov.tno.dal.db.entities.Action;
import ca.bc.gov.tno.models.AuditColumnModel;

public class ActionModel extends AuditColumnModel {
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
   * Type of value.
   */
  private ValueType valueType = ValueType.Boolean;

  /**
   * The label to display with the value.
   */
  private String valueLabel = "";

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * The value given to the action.
   */
  private String value;

  /**
   * A collection of role actions that belong to this action.
   */
  // private List<ContentAction> contentActions = new ArrayList<>();

  public ActionModel() {
  }

  public ActionModel(Action entity) {
    this(entity, "");
  }

  public ActionModel(Action entity, String value) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.valueType = entity.getValueType();
      this.valueLabel = entity.getValueLabel();
      this.enabled = entity.isEnabled();
      this.sortOrder = entity.getSortOrder();
    }
    this.value = value == null ? "" : value;
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
   * @return String the valueType
   */
  public ValueType getValueType() {
    return valueType;
  }

  /**
   * @param valueType the valueType to set
   */
  public void setValueType(ValueType valueType) {
    this.valueType = valueType;
  }

  /**
   * @return String the valueLabel
   */
  public String getValueLabel() {
    return valueLabel;
  }

  /**
   * @param valueLabel the valueLabel to set
   */
  public void setValueLabel(String valueLabel) {
    this.valueLabel = valueLabel;
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

  /**
   * @return String the value
   */
  public String getValue() {
    return value;
  }

  /**
   * @param sortOrder the value to set
   */
  public void setValue(String value) {
    this.value = value;
  }

}
