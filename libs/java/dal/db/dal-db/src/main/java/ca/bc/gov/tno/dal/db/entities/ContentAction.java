package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * ContentAction class, provides a way to manage content actions.
 */
@Entity
@IdClass(ContentActionPK.class)
@Table(name = "content_action", schema = "public")
public class ContentAction extends AuditColumns {
  /**
   * Primary key to identify the content action.
   * Foreign key to content.
   */
  @Id
  @Column(name = "content_id", nullable = false)
  private int contentId;

  /**
   * The content reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "content_id", insertable = false, updatable = false)
  private Content content;

  /**
   * Primary key to identify the content action.
   * Foreign key to action .
   */
  @Id
  @Column(name = "action_id", nullable = false)
  private int actionId;

  /**
   * The action reference.
   */
  @JsonBackReference("action")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "action_id", insertable = false, updatable = false)
  private Action action;

  /**
   * Value of action.
   */
  @Column(name = "value", nullable = false)
  private String value;

  /**
   * Creates a new instance of a ContentAction object.
   */
  public ContentAction() {

  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param content Content object
   * @param action  Action object
   * @param value   Action value
   */
  public ContentAction(Content content, Action action, String value) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (action == null)
      throw new NullPointerException("Parameter 'action' cannot be null.");
    if (value == null)
      throw new NullPointerException("Parameter 'value' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.action = action;
    this.actionId = action.getId();
    this.value = value;
  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param content Content object
   * @param action  Action object
   */
  public ContentAction(Content content, Action action) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (action == null)
      throw new NullPointerException("Parameter 'action' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.action = action;
    this.actionId = action.getId();
    this.value = "";
  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param actionId Foreign key to Action object
   * @param value    Action value
   */
  public ContentAction(Content content, int actionId, String value) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (value == null)
      throw new NullPointerException("Parameter 'value' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.actionId = actionId;
    this.value = value;
  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param actionId Foreign key to Action object
   */
  public ContentAction(Content content, int actionId) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.actionId = actionId;
    this.value = "";
  }

  /**
   * @return int return the contentId
   */
  public int getContentId() {
    return contentId;
  }

  /**
   * @param contentId the contentId to set
   */
  public void setContentId(int contentId) {
    this.contentId = contentId;
  }

  /**
   * @return Content return the content
   */
  public Content getContent() {
    return content;
  }

  /**
   * @param content the content to set
   */
  public void setContent(Content content) {
    this.content = content;
  }

  /**
   * @return int return the actionId
   */
  public int getActionId() {
    return actionId;
  }

  /**
   * @param actionId the actionId to set
   */
  public void setActionId(int actionId) {
    this.actionId = actionId;
  }

  /**
   * @return Action return the action
   */
  public Action getAction() {
    return action;
  }

  /**
   * @param action the action to set
   */
  public void setAction(Action action) {
    this.action = action;
  }

  /**
   * @return String return the value
   */
  public String getValue() {
    return value;
  }

  /**
   * @param value the value to set
   */
  public void setValue(String value) {
    this.value = value;
  }

}
