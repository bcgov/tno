package ca.bc.gov.tno.services.models;

/**
 * FileReference class, defines a reference to a file stored at some location.
 */
public class FileReference extends AuditColumns {
  /**
   * Primary key to identify the file reference.
   */
  private int id;

  /**
   * File path and name.
   */
  private String path;

  /**
   * File mimetype.
   */
  private String mimeType;

  /**
   * The size of the file in bytes.
   */
  private int size;

  /**
   * The total length of running time of the audio/video content.
   */
  private int runningTime;

  /**
   * Foreign key to the content.
   */
  private int contentId;

  /**
   * The file reference content reference.
   */
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
   * @param id          The primary key
   * @param content     The content the file belongs to
   * @param path        The path and name of the file
   * @param mimeType    The file mimetype
   * @param size        The size in bytes
   * @param runningTime The running time of audio/video in milliseconds
   */
  public FileReference(int id, Content content, String path, String mimeType, int size, int runningTime) {
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
    this.runningTime = runningTime;
  }

  /**
   * Creates a new instance of a FileReference object, initializes with specified
   * parameters.
   * 
   * @param id          The primary key
   * @param content     The content the file belongs to
   * @param path        The path and name of the file
   * @param mimeType    The file mimetype
   * @param size        The size in bytes
   * @param runningTime The running time of audio/video in milliseconds
   * @param version     Row version value
   */
  public FileReference(int id, Content content, String path, String mimeType, int size,
      int runningTime, long version) {
    this(id, content, path, mimeType, size, runningTime);
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
   * @return int return the runningTime
   */
  public int getRunningTime() {
    return runningTime;
  }

  /**
   * @param runningTime the runningTime to set
   */
  public void setRunningTime(int runningTime) {
    this.runningTime = runningTime;
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
