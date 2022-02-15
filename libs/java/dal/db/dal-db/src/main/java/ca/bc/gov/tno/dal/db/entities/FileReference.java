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

/**
 * FileReference class, defines a reference to a file stored at some location.
 */
@Entity
@Table(name = "file_reference", schema = "public")
public class FileReference extends AuditColumns {
  /**
   * Primary key to identify the file reference.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_file_reference")
  @SequenceGenerator(name = "seq_file_reference", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * File path and name.
   */
  @Column(name = "path", nullable = false)
  private String path;

  /**
   * File mimetype.
   */
  @Column(name = "mime_type", nullable = false)
  private String mimeType;

  /**
   * The size of the file in bytes.
   */
  @Column(name = "size")
  private int size;

  /**
   * The length of the video/audio in milliseconds.
   */
  @Column(name = "length", nullable = false)
  private int length;

  /**
   * Foreign key to the content.
   */
  @Column(name = "content_id", nullable = false)
  private int contentId;

  /**
   * The file reference content reference.
   */
  @JsonBackReference("content")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "content_id", insertable = false, updatable = false)
  private Content content;

  /**
   * Creates a new instance of a FileReference object.
   */
  public FileReference() {

  }

  /**
   * Creates a new instance of a FileReference object, initializes with specified
   * parameters.
   * 
   * @param id       The primary key
   * @param content  The content the file belongs to
   * @param path     The path and name of the file
   * @param mimeType The file mimetype
   * @param size     The size in bytes
   * @param length   The length of audio/video in milliseconds
   */
  public FileReference(int id, Content content, String path, String mimeType, int size, int length) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (path == null)
      throw new NullPointerException("Parameter 'path' cannot be null.");
    if (path.length() == 0)
      throw new IllegalArgumentException("Parameter 'path' cannot be empty.");
    if (mimeType == null)
      throw new NullPointerException("Parameter 'mimeType' cannot be null.");
    if (mimeType.length() == 0)
      throw new IllegalArgumentException("Parameter 'mimeType' cannot be empty.");

    this.id = id;
    this.path = path;
    this.mimeType = mimeType;
    this.content = content;
    this.contentId = content.getId();
    this.size = size;
    this.length = length;
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @return String return the path
   */
  public String getPath() {
    return path;
  }

  /**
   * @param path the path to set
   */
  public void setPath(String path) {
    this.path = path;
  }

  /**
   * @return String return the mimeType
   */
  public String getMimeType() {
    return mimeType;
  }

  /**
   * @param mimeType the mimeType to set
   */
  public void setMimeType(String mimeType) {
    this.mimeType = mimeType;
  }

  /**
   * @return int return the size
   */
  public int getSize() {
    return size;
  }

  /**
   * @param size the size to set
   */
  public void setSize(int size) {
    this.size = size;
  }

  /**
   * @return int return the length
   */
  public int getLength() {
    return length;
  }

  /**
   * @param length the length to set
   */
  public void setLength(int length) {
    this.length = length;
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

}
