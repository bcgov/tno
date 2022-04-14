package ca.bc.gov.tno.services.models;

/**
 * ContentAction class, provides a way to manage content actions.
 */
public class ContentAction extends AuditColumns {
  /**
   * Primary key to identify the content action.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * Primary key to identify the content action.
   * Foreign key to action .
   */
  private int actionId;

  /**
   * The action reference.
   */
  private Action action;

  /**
   * Value of action.
   */
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
   * @param value   Action value
   * @param version Row version value
   */
  public ContentAction(Content content, Action action, String value, long version) {
    this(content, action, value);
    this.setVersion(version);
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
   * @param value    Action value
   * @param version  Row version value
   */
  public ContentAction(Content content, int actionId, String value, long version) {
    this(content, actionId, value);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param actionId  Foreign key to Action object
   * @param value     Action value
   */
  public ContentAction(int contentId, int actionId, String value) {
    if (value == null)
      throw new NullPointerException("Parameter 'value' cannot be null.");

    this.contentId = contentId;
    this.actionId = actionId;
    this.value = value;
  }

  /**
   * Creates a new instance of a ContentAction object, initializes with specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param actionId  Foreign key to Action object
   * @param value     Action value
   * @param version   Row version value
   */
  public ContentAction(int contentId, int actionId, String value, long version) {
    this(contentId, actionId, value);
    this.setVersion(version);
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
