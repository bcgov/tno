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
import ca.bc.gov.tno.dal.db.ValueType;

/**
 * Action class, provides a way to manage actions.
 */
@Entity
@Table(name = "action", schema = "public")
public class Action extends AuditColumns {
  /**
   * Primary key to identify the action.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_action")
  @SequenceGenerator(name = "seq_action", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the action.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A description of the action.
   */
  @Column(name = "description")
  private String description;

  /**
   * Type of value.
   */
  @Column(name = "value_type", nullable = false)
  private ValueType valueType = ValueType.Boolean;

  /**
   * The label to display with the value.
   */
  @Column(name = "value_label", nullable = false)
  private String valueLabel = "";

  /**
   * The order to display.
   */
  @Column(name = "sort_order", nullable = false)
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of role actions that belong to this action.
   */
  @JsonBackReference("content_actions")
  @OneToMany(mappedBy = "action", fetch = FetchType.LAZY)
  private List<ContentAction> contentActions = new ArrayList<>();

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
   * @return int return the id
   */
  public int getId() {
    return id;
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
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
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

}
