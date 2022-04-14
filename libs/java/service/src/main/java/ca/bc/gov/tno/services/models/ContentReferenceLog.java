package ca.bc.gov.tno.services.models;

/**
 * ContentReferenceLog class, provides a way to store content reference logs.
 */
public class ContentReferenceLog extends AuditColumns {
  /**
   * Primary key to identify the content reference log.
   */
  private long id;

  /**
   * Foreign key to the content reference.
   */
  private String source;

  /**
   * Foreign key to the content reference.
   */
  private String uid;

  /**
   * The content reference.
   */
  private ContentReference contentReference;

  /**
   * The workflow process status of the content.
   */
  private WorkflowStatus workflowStatus = WorkflowStatus.InProgress;

  /**
   * A message describing the log event.
   */
  private String message;

  /**
   * Creates a new instance of a ContentReferenceLog object.
   */
  public ContentReferenceLog() {

  }

  /**
   * Creates a new instance of a ContentReferenceLog object, initializes with
   * specified parameters.
   *
   * @param source         Foreign key to content reference
   * @param uid            Foreign key to content reference
   * @param workflowStatus The workflow status
   * @param message        The log message
   */
  public ContentReferenceLog(String source, String uid, WorkflowStatus workflowStatus, String message) {
    if (source == null)
      throw new NullPointerException("Parameter 'source' cannot be null.");
    if (source.length() == 0)
      throw new IllegalArgumentException("Parameter 'source' cannot be empty.");
    if (uid == null)
      throw new NullPointerException("Parameter 'uid' cannot be null.");
    if (uid.length() == 0)
      throw new IllegalArgumentException("Parameter 'uid' cannot be empty.");
    if (workflowStatus == null)
      throw new NullPointerException("Parameter 'workflowStatus' cannot be null.");
    if (message == null)
      throw new NullPointerException("Parameter 'message' cannot be null.");
    if (message.length() == 0)
      throw new IllegalArgumentException("Parameter 'message' cannot be empty.");

    this.source = source;
    this.uid = uid;
    this.workflowStatus = workflowStatus;
    this.message = message;
  }

  /**
   * Creates a new instance of a ContentReferenceLog object, initializes with
   * specified parameters.
   *
   * @param contentRef Foreign key to content
   * @param message    The log message
   */
  public ContentReferenceLog(ContentReference contentRef, String message) {
    if (contentRef == null)
      throw new NullPointerException("Parameter 'contentRef' cannot be null.");
    if (message == null)
      throw new NullPointerException("Parameter 'message' cannot be null.");
    if (message.length() == 0)
      throw new IllegalArgumentException("Parameter 'message' cannot be empty.");

    this.contentReference = contentRef;
    this.source = contentRef.getSource();
    this.uid = contentRef.getUid();
    this.workflowStatus = contentRef.getWorkflowStatus();
    this.message = message;
  }

  /**
   * @return long return the id
   */
  public long getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(long id) {
    this.id = id;
  }

  /**
   * @return String return the source
   */
  public String getSource() {
    return source;
  }

  /**
   * @param source the source to set
   */
  public void setSource(String source) {
    this.source = source;
  }

  /**
   * @return String return the uid
   */
  public String getUid() {
    return uid;
  }

  /**
   * @param uid the uid to set
   */
  public void setUid(String uid) {
    this.uid = uid;
  }

  /**
   * @return ContentReference return the contentReference
   */
  public ContentReference getContentReference() {
    return contentReference;
  }

  /**
   * @param contentReference the contentReference to set
   */
  public void setContentReference(ContentReference contentReference) {
    this.contentReference = contentReference;
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
