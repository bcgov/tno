package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.ContentStatus;
import ca.bc.gov.tno.dal.db.WorkflowStatus;

/**
 * ContentLog class, provides a way to store content logs.
 */
@Entity
@Table(name = "content_log", schema = "public")
public class ContentLog extends AuditColumns {
  /**
   * Primary key to identify the content log.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_content_log")
  @SequenceGenerator(name = "seq_content_log", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * The status of the content.
   */
  @Column(name = "status", nullable = false)
  private ContentStatus status = ContentStatus.Draft;

  /**
   * The workflow process status of the content.
   */
  @Column(name = "workflow_status", nullable = false)
  private WorkflowStatus workflowStatus = WorkflowStatus.InProgress;

  /**
   * Foreign key to the content.
   */
  @Column(name = "content_id", nullable = false)
  private int contentId;

  /**
   * The content reference.
   */
  @JsonBackReference("content")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "content_id", insertable = false, updatable = false)
  private Content content;

  /**
   * A message describing the log event.
   */
  @Column(name = "message", nullable = false)
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

}
