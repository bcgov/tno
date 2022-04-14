package ca.bc.gov.tno.services.models;

/**
 * ContentLog class, provides a way to store content logs.
 */
public class ContentLog extends AuditColumns {
  /**
   * Primary key to identify the content log.
   */
  private int id;

  /**
   * The status of the content.
   */
  private ContentStatus status = ContentStatus.Draft;

  /**
   * The workflow process status of the content.
   */
  private WorkflowStatus workflowStatus = WorkflowStatus.InProgress;

  /**
   * Foreign key to the content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * A message describing the log event.
   */
  private String message;

  /**
   * Creates a new instance of a ContentLog object.
   */
  public ContentLog() {

  }

  /**
   * Creates a new instance of a ContentLog object, initializes with specified
   * parameters.
   *
   * @param contentId      Foreign key to content
   * @param status         The status
   * @param workflowStatus The workflow status
   * @param message        The log message
   */
  public ContentLog(int contentId, ContentStatus status, WorkflowStatus workflowStatus, String message) {
    if (status == null)
      throw new NullPointerException("Parameter 'status' cannot be null.");
    if (workflowStatus == null)
      throw new NullPointerException("Parameter 'workflowStatus' cannot be null.");
    if (message == null)
      throw new NullPointerException("Parameter 'message' cannot be null.");
    if (message.length() == 0)
      throw new IllegalArgumentException("Parameter 'message' cannot be empty.");

    this.contentId = contentId;
    this.status = status;
    this.workflowStatus = workflowStatus;
    this.message = message;
  }

  /**
   * Creates a new instance of a ContentLog object, initializes with specified
   * parameters.
   *
   * @param content Foreign key to content
   * @param message The log message
   */
  public ContentLog(Content content, String message) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (message == null)
      throw new NullPointerException("Parameter 'message' cannot be null.");
    if (message.length() == 0)
      throw new IllegalArgumentException("Parameter 'message' cannot be empty.");

    this.content = content;
    this.contentId = content.getId();
    this.status = content.getStatus();
    this.workflowStatus = content.getWorkflowStatus();
    this.message = message;
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
   * @return ContentStatus return the status
   */
  public ContentStatus getStatus() {
    return status;
  }

  /**
   * @param status the status to set
   */
  public void setStatus(ContentStatus status) {
    this.status = status;
  }

  /**
   * @return WorkflowStatus return the workflowStatus
   */
  public WorkflowStatus getWorkflowStatus() {
    return workflowStatus;
  }

  /**
   * @param workflowStatus the workflowStatus to set
   */
  public void setWorkflowStatus(WorkflowStatus workflowStatus) {
    this.workflowStatus = workflowStatus;
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
   * @return String return the message
   */
  public String getMessage() {
    return message;
  }

  /**
   * @param message the message to set
   */
  public void setMessage(String message) {
    this.message = message;
  }

}
