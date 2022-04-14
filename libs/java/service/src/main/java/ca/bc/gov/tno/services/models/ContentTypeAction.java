package ca.bc.gov.tno.services.models;

/**
 * ContentTypeAction class, provides a way to manage content actions.
 */
public class ContentTypeAction extends AuditColumns {
  /**
   * Primary key to identify the content type action.
   * Foreign key to content type.
   */
  private int contentTypeId;

  /**
   * The content reference.
   */
  private ContentType contentType;

  /**
   * Primary key to identify the content type action.
   * Foreign key to action .
   */
  private int actionId;

  /**
   * The action reference.
   */
  private Action action;

  /**
   * Creates a new instance of a ContentTypeAction object.
   */
  public ContentTypeAction() {

  }

  /**
   * Creates a new instance of a ContentTypeAction object, initializes with
   * specified
   * parameters.
   * 
   * @param contentType ContentType object
   * @param action      Action object
   */
  public ContentTypeAction(ContentType contentType, Action action) {
    if (contentType == null)
      throw new NullPointerException("Parameter 'contentType' cannot be null.");
    if (action == null)
      throw new NullPointerException("Parameter 'action' cannot be null.");

    this.contentType = contentType;
    this.contentTypeId = contentType.getId();
    this.action = action;
    this.actionId = action.getId();
  }

  /**
   * Creates a new instance of a ContentTypeAction object, initializes with
   * specified
   * parameters.
   * 
   * @param contentType ContentType object
   * @param action      Action object
   * @param version     Row version value
   */
  public ContentTypeAction(ContentType contentType, Action action, long version) {
    this(contentType, action);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a ContentTypeAction object, initializes with
   * specified
   * parameters.
   * 
   * @param contentType ContentType object
   * @param actionId    Foreign key to Action object
   */
  public ContentTypeAction(ContentType contentType, int actionId) {
    if (contentType == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");

    this.contentType = contentType;
    this.contentTypeId = contentType.getId();
    this.actionId = actionId;
  }

  /**
   * Creates a new instance of a ContentTypeAction object, initializes with
   * specified
   * parameters.
   * 
   * @param contentType ContentType object
   * @param actionId    Foreign key to Action object
   * @param version     Row version value
   */
  public ContentTypeAction(ContentType contentType, int actionId, long version) {
    this(contentType, actionId);
    this.setVersion(version);
  }

  /**
   * @return int return the contentTypeId
   */
  public int getContentTypeId() {
    return contentTypeId;
  }

  /**
   * @param contentTypeId the contentTypeId to set
   */
  public void setContentTypeId(int contentTypeId) {
    this.contentTypeId = contentTypeId;
  }

  /**
   * @return ContentType return the contentType
   */
  public ContentType getContentType() {
    return contentType;
  }

  /**
   * @param contentType the contentType to set
   */
  public void setContentType(ContentType contentType) {
    this.contentType = contentType;
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
}
