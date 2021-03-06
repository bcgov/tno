package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * Action class, provides a way to identify actions that are performed on or
 * associated with content.
 */
public class Action extends AuditColumns {
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
  private boolean isEnabled = true;

  /**
   * A collection of role actions that belong to this action.
   */
  private List<ContentAction> contentActions = new ArrayList<>();

  /**
   * A collection of content types for this action.
   */
  private List<ContentTypeAction> contentTypeActions = new ArrayList<>();

  /**
   * Creates a new instance of a Action object.
   */
  public Action() {

  }

  /**
   * Creates a new instance of a Action object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public Action(int id, String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.id = id;
    this.name = name;
    this.valueType = ValueType.Boolean;
  }

  /**
   * Creates a new instance of a Action object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public Action(int id, String name, long version) {
    this(id, name);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a Action object, initializes with specified
   * parameters.
   *
   * @param id         Primary key
   * @param name       Unique name
   * @param valueType  Type of value allowed
   * @param valueLabel Label for value
   */
  public Action(int id, String name, ValueType valueType, String valueLabel) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (valueLabel == null)
      throw new NullPointerException("Parameter 'valueLabel' cannot be null.");
    if (valueLabel.length() == 0)
      throw new IllegalArgumentException("Parameter 'valueLabel' cannot be empty.");

    this.id = id;
    this.name = name;
    this.valueType = valueType;
    this.valueLabel = valueLabel;
  }

  /**
   * Creates a new instance of a Action object, initializes with specified
   * parameters.
   *
   * @param id         Primary key
   * @param name       Unique name
   * @param valueType  Type of value allowed
   * @param valueLabel Label for value
   * @param version    Row version value
   */
  public Action(int id, String name, ValueType valueType, String valueLabel, long version) {
    this(id, name, valueType, valueLabel);
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
   * @return ValueType return the valueType
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
   * @return String return the valueLabel
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
   * @return List{ContentAction} return the contentActions
   */
  public List<ContentAction> getContentActions() {
    return contentActions;
  }

  /**
   * @param contentActions the contentActions to set
   */
  public void setContentActions(List<ContentAction> contentActions) {
    this.contentActions = contentActions;
  }

  /**
   * @return List{ContentTypeAction} return the contentTypeActions
   */
  public List<ContentTypeAction> getContentTypeActions() {
    return contentTypeActions;
  }

  /**
   * @param contentTypeActions the contentTypeActions to set
   */
  public void setContentTypeActions(List<ContentTypeAction> contentTypeActions) {
    this.contentTypeActions = contentTypeActions;
  }

}
