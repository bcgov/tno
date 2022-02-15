package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.JoinColumns;
import javax.persistence.ManyToOne;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.WorkflowStatus;

/**
 * ContentReferenceLog class, provides a way to store content reference logs.
 */
@Entity
@Table(name = "content_reference_log", schema = "public")
public class ContentReferenceLog extends AuditColumns {
  /**
   * Primary key to identify the content reference log.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_content_reference_log")
  @SequenceGenerator(name = "seq_content_reference_log", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * Foreign key to the content reference.
   */
  @Column(name = "source", nullable = false)
  private String source;

  /**
   * Foreign key to the content reference.
   */
  @Column(name = "uid", nullable = false)
  private String uid;

  /**
   * The content reference.
   */
  @JsonBackReference("contentReference")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumns({
      @JoinColumn(name = "source", referencedColumnName = "source", insertable = false, updatable = false),
      @JoinColumn(name = "uid", referencedColumnName = "uid", insertable = false, updatable = false)
  })
  private ContentReference contentReference;

  /**
   * The workflow process status of the content.
   */
  @Column(name = "status", nullable = false)
  private WorkflowStatus status = WorkflowStatus.InProgress;

  /**
   * A message describing the log event.
   */
  @Column(name = "message", nullable = false)
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
   * @param source  Foreign key to content reference
   * @param uid     Foreign key to content reference
   * @param status  The status
   * @param message The log message
   */
  public ContentReferenceLog(String source, String uid, WorkflowStatus status, String message) {
    if (source == null)
      throw new NullPointerException("Parameter 'source' cannot be null.");
    if (source.length() == 0)
      throw new IllegalArgumentException("Parameter 'source' cannot be empty.");
    if (uid == null)
      throw new NullPointerException("Parameter 'uid' cannot be null.");
    if (uid.length() == 0)
      throw new IllegalArgumentException("Parameter 'uid' cannot be empty.");
    if (status == null)
      throw new NullPointerException("Parameter 'status' cannot be null.");
    if (message == null)
      throw new NullPointerException("Parameter 'message' cannot be null.");
    if (message.length() == 0)
      throw new IllegalArgumentException("Parameter 'message' cannot be empty.");

    this.source = source;
    this.uid = uid;
    this.status = status;
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
    this.status = contentRef.getStatus();
    this.message = message;
  }

}
